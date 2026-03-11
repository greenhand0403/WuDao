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
using WuDao.Content.Global.Projectiles;

namespace WuDao.Content.Systems
{
    // 隐藏火球和暗影火球的光照和粒子效果，但是没有成功。
    // 但是好像不需要，之前的代码已经能隐藏了。
    // 到此为止吧，后续再仔细研究拜月邪教徒
    // ① 不加载 ② 在工程文件里面remove掉这个文件，不编译这个类
    [Autoload(false)]
    public class InvisibleEnemiesProjectileAI001HookSystem : ModSystem
    {
        private static ILHook ai001Hook;

        public override void Load()
        {
            MethodInfo ai001Method = typeof(Projectile).GetMethod(
                "AI_001",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            if (ai001Method == null)
            {
                Mod.Logger.Warn("Failed to find Terraria.Projectile.AI_001");
                return;
            }

            ai001Hook = new ILHook(ai001Method, Projectile_AI001_IL);
        }

        public override void Unload()
        {
            ai001Hook?.Dispose();
            ai001Hook = null;
        }

        private static void Projectile_AI001_IL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel continueOriginal = il.DefineLabel();

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Projectile, bool>>(TryRunHiddenAI001WithoutVisuals);

            c.Emit(OpCodes.Brfalse_S, continueOriginal);
            c.Emit(OpCodes.Ret);

            c.MarkLabel(continueOriginal);
        }

        private static bool TryRunHiddenAI001WithoutVisuals(Projectile projectile)
        {
            if (projectile == null || !projectile.active)
                return false;

            // if (projectile.type != ProjectileID.CultistBossFireBall && projectile.type != ProjectileID.CultistBossFireBallClone)
            //     return false;

            if (!InvisibleEnemiesGlobalProjectile.ShouldHide(projectile))
                return false;

            RunHiddenAI001FireballWithoutVisuals(projectile);
            return true;
        }
        private static void RunHiddenAI001FireballWithoutVisuals(Projectile projectile)
        {
            if (projectile.type == ProjectileID.CultistBossFireBall)
            {
                RunHiddenFireball467WithoutVisuals(projectile);
                return;
            }

            if (projectile.type == ProjectileID.CultistBossFireBallClone)
            {
                RunHiddenFireball468WithoutVisuals(projectile);
                return;
            }
        }
        private static void RunHiddenFireball467WithoutVisuals(Projectile projectile)
        {
            if (projectile.ai[1] == 0f)
            {
                projectile.ai[1] = 1f;
                SoundEngine.PlaySound(in SoundID.Item34, projectile.position);
            }
            else if (projectile.ai[1] == 1f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int targetIndex = -1;
                float nearestDistance = 2000f;

                for (int k = 0; k < 255; k++)
                {
                    if (Main.player[k].active && !Main.player[k].dead)
                    {
                        Vector2 center = Main.player[k].Center;
                        float distance = Vector2.Distance(center, projectile.Center);

                        if ((distance < nearestDistance || targetIndex == -1) &&
                            Collision.CanHit(projectile.Center, 1, 1, center, 1, 1))
                        {
                            nearestDistance = distance;
                            targetIndex = k;
                        }
                    }
                }

                if (nearestDistance < 20f)
                {
                    projectile.Kill();
                    return;
                }

                if (targetIndex != -1)
                {
                    projectile.ai[1] = 21f;
                    projectile.ai[0] = targetIndex;
                    projectile.netUpdate = true;
                }
            }
            else if (projectile.ai[1] > 20f && projectile.ai[1] < 200f)
            {
                projectile.ai[1] += 1f;
                int playerIndex = (int)projectile.ai[0];

                if (!Main.player[playerIndex].active || Main.player[playerIndex].dead)
                {
                    projectile.ai[1] = 1f;
                    projectile.ai[0] = 0f;
                    projectile.netUpdate = true;
                }
                else
                {
                    float currentRotation = projectile.velocity.ToRotation();
                    Vector2 toTarget = Main.player[playerIndex].Center - projectile.Center;

                    if (toTarget.Length() < 20f)
                    {
                        projectile.Kill();
                        return;
                    }

                    float targetAngle = toTarget.ToRotation();
                    if (toTarget == Vector2.Zero)
                    {
                        targetAngle = currentRotation;
                    }

                    float newRotation = currentRotation.AngleLerp(targetAngle, 0.008f);
                    projectile.velocity = new Vector2(projectile.velocity.Length(), 0f).RotatedBy(newRotation);
                }
            }

            if (projectile.ai[1] >= 1f && projectile.ai[1] < 20f)
            {
                projectile.ai[1] += 1f;
                if (projectile.ai[1] == 20f)
                {
                    projectile.ai[1] = 1f;
                }
            }

            projectile.alpha -= 40;
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }

            projectile.spriteDirection = projectile.direction;

            projectile.frameCounter++;
            if (projectile.frameCounter >= 3)
            {
                projectile.frame++;
                projectile.frameCounter = 0;
                if (projectile.frame >= 4)
                {
                    projectile.frame = 0;
                }
            }

            // 原版这里有:
            // Lighting.AddLight(projectile.Center, 1.1f, 0.9f, 0.4f);

            projectile.localAI[0] += 1f;

            if (projectile.localAI[0] == 12f)
            {
                projectile.localAI[0] = 0f;

                // 原版这里生成 12 个 Dust.NewDust(type 6)
                // 故意跳过
            }

            if (Main.rand.Next(4) == 0)
            {
                // 原版这里生成 Dust.NewDust(type 31)
                // 故意跳过
            }

            if (Main.rand.Next(32) == 0)
            {
                // 原版这里生成 Dust.NewDust(type 31)
                // 故意跳过
            }

            if (Main.rand.Next(2) == 0)
            {
                // 原版这里生成 Dust.NewDust(type 6)
                // 故意跳过
            }
        }

        private static void RunHiddenFireball468WithoutVisuals(Projectile projectile)
        {
            if (projectile.ai[1] == 0f)
            {
                projectile.ai[1] = 1f;
                SoundEngine.PlaySound(in SoundID.Item34, projectile.position);
            }
            else if (projectile.ai[1] == 1f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int targetIndex = -1;
                float nearestDistance = 2000f;

                for (int k = 0; k < 255; k++)
                {
                    if (Main.player[k].active && !Main.player[k].dead)
                    {
                        Vector2 center = Main.player[k].Center;
                        float distance = Vector2.Distance(center, projectile.Center);

                        if ((distance < nearestDistance || targetIndex == -1) &&
                            Collision.CanHit(projectile.Center, 1, 1, center, 1, 1))
                        {
                            nearestDistance = distance;
                            targetIndex = k;
                        }
                    }
                }

                if (nearestDistance < 20f)
                {
                    projectile.Kill();
                    return;
                }

                if (targetIndex != -1)
                {
                    projectile.ai[1] = 21f;
                    projectile.ai[0] = targetIndex;
                    projectile.netUpdate = true;
                }
            }
            else if (projectile.ai[1] > 20f && projectile.ai[1] < 200f)
            {
                projectile.ai[1] += 1f;
                int playerIndex = (int)projectile.ai[0];

                if (!Main.player[playerIndex].active || Main.player[playerIndex].dead)
                {
                    projectile.ai[1] = 1f;
                    projectile.ai[0] = 0f;
                    projectile.netUpdate = true;
                }
                else
                {
                    float currentRotation = projectile.velocity.ToRotation();
                    Vector2 toTarget = Main.player[playerIndex].Center - projectile.Center;

                    if (toTarget.Length() < 20f)
                    {
                        projectile.Kill();
                        return;
                    }

                    float targetAngle = toTarget.ToRotation();
                    if (toTarget == Vector2.Zero)
                    {
                        targetAngle = currentRotation;
                    }

                    float newRotation = currentRotation.AngleLerp(targetAngle, 0.01f);
                    projectile.velocity = new Vector2(projectile.velocity.Length(), 0f).RotatedBy(newRotation);
                }
            }

            if (projectile.ai[1] >= 1f && projectile.ai[1] < 20f)
            {
                projectile.ai[1] += 1f;
                if (projectile.ai[1] == 20f)
                {
                    projectile.ai[1] = 1f;
                }
            }

            projectile.alpha -= 40;
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }

            projectile.spriteDirection = projectile.direction;

            projectile.frameCounter++;
            if (projectile.frameCounter >= 3)
            {
                projectile.frame++;
                projectile.frameCounter = 0;
                if (projectile.frame >= 4)
                {
                    projectile.frame = 0;
                }
            }

            // 原版这里有:
            // Lighting.AddLight(projectile.Center, 0.2f, 0.1f, 0.6f);

            projectile.localAI[0] += 1f;

            if (projectile.localAI[0] == 12f)
            {
                projectile.localAI[0] = 0f;

                // 原版这里生成 12 个 Dust.NewDust(type 27)
                // 故意跳过
            }

            if (Main.rand.Next(4) == 0)
            {
                // 原版这里生成 Dust.NewDust(type 31)
                // 故意跳过
            }

            if (Main.rand.Next(32) == 0)
            {
                // 原版这里生成 Dust.NewDust(type 31)
                // 故意跳过
            }

            if (Main.rand.Next(2) == 0)
            {
                // 原版这里生成 Dust.NewDust(type 27)
                // 故意跳过
            }
        }
    }
}