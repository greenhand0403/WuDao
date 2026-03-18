using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using WuDao.Content.Global.Projectiles;

namespace WuDao.Content.Systems
{
    // 参考 https://github.com/tModLoader/tModLoader/wiki/Advanced-Detouring-Guide
    // 隐藏冰雪雾成功，但是隐藏闪电球的光照和粒子效果，但是没有成功。推测闪电珠和闪电珠弧的粒子可能在其他地方生成。到此为止吧，后续再仔细研究拜月邪教徒
    public class InvisibleEnemiesProjectileAIHookSystem : ModSystem
    {
        private static ILHook vanillaAIHook;

        public override void Load()
        {
            MethodInfo vanillaAIMethod = typeof(Projectile).GetMethod(
                "VanillaAI",
                BindingFlags.Instance | BindingFlags.Public
            );

            if (vanillaAIMethod == null)
            {
                Mod.Logger.Warn("Failed to find Terraria.Projectile.VanillaAI");
                return;
            }

            vanillaAIHook = new ILHook(vanillaAIMethod, Projectile_VanillaAI_IL);
        }

        public override void Unload()
        {
            vanillaAIHook?.Dispose();
            vanillaAIHook = null;
        }

        private static void Projectile_VanillaAI_IL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel continueOriginal = il.DefineLabel();

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Projectile, bool>>(TryRunHiddenIceMistAndLightingOrbAI);

            c.Emit(OpCodes.Brfalse_S, continueOriginal);
            c.Emit(OpCodes.Ret);

            c.MarkLabel(continueOriginal);
        }

        /// <summary>
        /// 返回 true 表示已经接管了该 projectile 的 VanillaAI，应直接 return。
        /// 返回 false 表示继续执行原版 VanillaAI。
        /// </summary>
        private static bool TryRunHiddenIceMistAndLightingOrbAI(Projectile projectile)
        {
            if (projectile == null || !projectile.active)
                return false;

            if (!InvisibleEnemiesGlobalProjectile.ShouldHide(projectile))
                return false;

            // aiStyle 86: Ice Mist
            if (projectile.aiStyle == ProjAIStyleID.IceMist)
            {
                RunHiddenIceMistAIWithoutVisuals(projectile);
                return true;
            }

            // aiStyle 88: Lightning Orb / Lightning Arc
            // if (projectile.aiStyle == ProjAIStyleID.LightningOrb)
            // {
            //     RunHiddenLightningOrbStyleAIWithoutVisuals(projectile);
            //     return true;
            // }

            return false;
        }

        /// <summary>
        /// 基于原版 aiStyle == 86 逻辑，保留行为，移除 Dust.NewDust 和 Lighting.AddLight。
        /// </summary>
        private static void RunHiddenIceMistAIWithoutVisuals(Projectile projectile)
        {
            if (projectile.type != ProjectileID.CultistBossIceMist)
                return;

            if (projectile.localAI[1] == 0f)
            {
                projectile.localAI[1] = 1f;
                SoundEngine.PlaySound(in SoundID.Item120, projectile.position);
            }

            projectile.ai[0]++;

            if (projectile.ai[1] == 1f)
            {
                if (projectile.ai[0] >= 130f)
                    projectile.alpha += 10;
                else
                    projectile.alpha -= 10;

                if (projectile.alpha < 0)
                    projectile.alpha = 0;

                if (projectile.alpha > 255)
                    projectile.alpha = 255;

                if (projectile.ai[0] >= 150f)
                {
                    projectile.Kill();
                    return;
                }

                if (projectile.ai[0] % 30f == 0f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 dir = projectile.rotation.ToRotationVector2();
                    Projectile.NewProjectile(
                        projectile.GetSource_FromThis(),
                        projectile.Center.X,
                        projectile.Center.Y,
                        dir.X,
                        dir.Y,
                        ProjectileID.CultistBossIceMist,
                        projectile.damage,
                        projectile.knockBack,
                        projectile.owner
                    );
                }

                projectile.rotation += (float)Math.PI / 30f;

                // 原版这里有 Lighting.AddLight(base.Center, 0.3f, 0.75f, 0.9f);
                // 故意跳过
                return;
            }

            projectile.position -= projectile.velocity;

            if (projectile.ai[0] >= 40f)
                projectile.alpha += 3;
            else
                projectile.alpha -= 40;

            if (projectile.alpha < 0)
                projectile.alpha = 0;

            if (projectile.alpha > 255)
                projectile.alpha = 255;

            if (projectile.ai[0] >= 45f)
            {
                projectile.Kill();
                return;
            }

            // 原版这里会计算大范围旋转轨迹，并在上面 Lighting.AddLight + Dust.NewDust
            // 这里整体省略，只保留弹幕本身行为，不再生成视觉特效。
        }

        private static void RunHiddenLightningOrbStyleAIWithoutVisuals(Projectile projectile)
        {
            if (projectile.type == ProjectileID.CultistBossLightningOrb)
            {
                if (projectile.localAI[1] == 0f)
                {
                    SoundEngine.PlaySound(in SoundID.Item121, projectile.position);
                    projectile.localAI[1] = 1f;
                }

                if (projectile.ai[0] < 180f)
                {
                    projectile.alpha -= 5;
                    if (projectile.alpha < 0)
                        projectile.alpha = 0;
                }
                else
                {
                    projectile.alpha += 5;
                    if (projectile.alpha > 255)
                    {
                        projectile.alpha = 255;
                        projectile.Kill();
                        return;
                    }
                }

                projectile.ai[0]++;

                if (projectile.ai[0] % 30f == 0f && projectile.ai[0] < 180f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int[] targets = new int[5];
                    Vector2[] targetCenters = new Vector2[5];
                    int found = 0;
                    float bestDist = 2000f;

                    for (int i = 0; i < 255; i++)
                    {
                        if (!Main.player[i].active || Main.player[i].dead)
                            continue;

                        Vector2 center = Main.player[i].Center;
                        float dist = Vector2.Distance(center, projectile.Center);
                        if (dist < bestDist && Collision.CanHit(projectile.Center, 1, 1, center, 1, 1))
                        {
                            targets[found] = i;
                            targetCenters[found] = center;
                            found++;
                            if (found >= targetCenters.Length)
                                break;
                        }
                    }

                    for (int i = 0; i < found; i++)
                    {
                        Vector2 toTarget = targetCenters[i] - projectile.Center;
                        float ai = Main.rand.Next(100);
                        Vector2 vel = Vector2.Normalize(toTarget.RotatedByRandom(0.7853981852531433)) * 7f;
                        Projectile.NewProjectile(
                            projectile.GetSource_FromThis(),
                            projectile.Center.X,
                            projectile.Center.Y,
                            vel.X,
                            vel.Y,
                            ProjectileID.CultistBossLightningOrbArc,
                            projectile.damage,
                            0f,
                            Main.myPlayer,
                            toTarget.ToRotation(),
                            ai
                        );
                    }
                }

                if (++projectile.frameCounter >= 4)
                {
                    projectile.frameCounter = 0;
                    if (++projectile.frame >= Main.projFrames[projectile.type])
                        projectile.frame = 0;
                }

                // 故意跳过 Lighting.AddLight 和 Dust.NewDust
                return;
            }

            if (projectile.type == ProjectileID.CultistBossLightningOrbArc)
            {
                projectile.frameCounter++;

                if (projectile.velocity == Vector2.Zero)
                {
                    if (projectile.frameCounter >= projectile.extraUpdates * 2)
                    {
                        projectile.frameCounter = 0;
                        bool allSame = true;
                        for (int i = 1; i < projectile.oldPos.Length; i++)
                        {
                            if (projectile.oldPos[i] != projectile.oldPos[0])
                                allSame = false;
                        }

                        if (allSame)
                        {
                            projectile.Kill();
                            return;
                        }
                    }

                    // 故意跳过 Dust.NewDust
                }
                else
                {
                    if (projectile.frameCounter < projectile.extraUpdates * 2)
                        return;

                    projectile.frameCounter = 0;
                    float speed = projectile.velocity.Length();
                    UnifiedRandom rand = new UnifiedRandom((int)projectile.ai[1]);
                    int tries = 0;
                    Vector2 chosen = -Vector2.UnitY;

                    while (true)
                    {
                        int next = rand.Next();
                        projectile.ai[1] = next;
                        next %= 100;

                        float f = (float)next / 100f * ((float)Math.PI * 2f);
                        Vector2 v = f.ToRotationVector2();
                        if (v.Y > 0f)
                            v.Y *= -1f;

                        bool bad = false;
                        if (v.Y > -0.02f)
                            bad = true;
                        if (v.X * (projectile.extraUpdates + 1) * 2f * speed + projectile.localAI[0] > 40f)
                            bad = true;
                        if (v.X * (projectile.extraUpdates + 1) * 2f * speed + projectile.localAI[0] < -40f)
                            bad = true;

                        if (bad)
                        {
                            if (tries++ >= 100)
                            {
                                projectile.velocity = Vector2.Zero;
                                projectile.localAI[1] = 1f;
                                break;
                            }
                            continue;
                        }

                        chosen = v;
                        break;
                    }

                    if (projectile.velocity != Vector2.Zero)
                    {
                        projectile.localAI[0] += chosen.X * (projectile.extraUpdates + 1) * 2f * speed;
                        projectile.velocity = chosen.RotatedBy(projectile.ai[0] + (float)Math.PI / 2f) * speed;
                        projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2f;
                    }
                }

                // 故意跳过 Lighting.AddLight 和 Dust.NewDust
                return;
            }
        }
    }
}