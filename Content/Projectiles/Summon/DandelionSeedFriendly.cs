using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Summon
{
    public class DandelionSeedFriendly : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DandelionSeed;

        private const int FrameCount = 4;
        private const int FrameWidth = 14;
        private const int FrameHeight = 20;

        private const float SearchDistance = 520f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = FrameCount;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 20;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 480;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.DamageType = DamageClass.Summon;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;

                for (int i = 0; i < 3; i++)
                {
                    int idx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0f, 0f, 50, Color.White, 1.2f);
                    Dust dust = Main.dust[idx];
                    dust.velocity *= 0.3f;
                    dust.noGravity = true;
                }

                // 初速度：先向上飘一点
                Projectile.velocity.Y -= Main.rand.NextFloat(1.2f, 2.0f);
                Projectile.velocity.X += Main.rand.NextFloat(-0.4f, 0.4f);
            }

            AnimateFrames();

            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 25;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }

            if (!Projectile.friendly)
            {
                Projectile.velocity *= 0.88f;
                Projectile.alpha += 30;
                return;
            }

            NPC target = FindTarget(SearchDistance);

            float windX = Main.WindForVisuals;
            int windDir = windX > 0f ? 1 : -1;

            if (windX == 0f)
                windDir = Main.rand.NextBool() ? 1 : -1;

            Projectile.spriteDirection = windDir;

            // localAI[1] 当作阶段计时
            Projectile.localAI[1]++;

            bool risingPhase = Projectile.localAI[1] < 22f;   // 前期先上飘
            bool glidePhase = Projectile.localAI[1] >= 22f;   // 后期开始缓慢下落并靠向目标

            float maxX = 2.3f;
            float maxUp = 1.8f;
            float maxDown = 2.2f;

            if (target != null)
            {
                int targetDir = target.Center.X > Projectile.Center.X ? 1 : -1;
                bool windSameSide = windDir == targetDir;

                float xAccel = windSameSide ? 0.055f : 0.03f;
                float yFallAccel = windSameSide ? 0.045f : 0.03f;
                float yRiseAccel = windSameSide ? 0.08f : 0.05f;

                // 横向：顺风并缓慢朝目标靠
                Projectile.velocity.X += xAccel * targetDir * (0.6f + Math.Abs(windX));
                Projectile.velocity.X += windX * 0.02f;

                if (Projectile.velocity.X > maxX)
                    Projectile.velocity.X -= 0.08f;
                if (Projectile.velocity.X < -maxX)
                    Projectile.velocity.X += 0.08f;

                if (risingPhase)
                {
                    Projectile.velocity.Y -= 0.08f;
                    if (Projectile.velocity.Y < -maxUp)
                        Projectile.velocity.Y += 0.12f;
                }
                else
                {
                    // 高于目标太多：开始下落
                    if (target.Top.Y >= Projectile.Center.Y || !windSameSide)
                    {
                        Projectile.velocity.Y += yFallAccel;
                        if (Projectile.velocity.Y > maxDown)
                            Projectile.velocity.Y -= 0.08f;
                    }
                    else
                    {
                        // 目标还在上方时，稍微再抬一点，但别变成直线追踪
                        Projectile.velocity.Y -= yRiseAccel;
                        if (Projectile.velocity.Y < -1.2f)
                            Projectile.velocity.Y += 0.16f;
                    }
                }
            }
            else
            {
                // 没目标时：单纯像蒲公英种子一样飘
                Projectile.velocity.X += windX * 0.03f;
                if (Projectile.velocity.X > maxX)
                    Projectile.velocity.X -= 0.06f;
                if (Projectile.velocity.X < -maxX)
                    Projectile.velocity.X += 0.06f;

                if (risingPhase)
                {
                    Projectile.velocity.Y -= 0.07f;
                    if (Projectile.velocity.Y < -maxUp)
                        Projectile.velocity.Y += 0.12f;
                }
                else if (glidePhase)
                {
                    Projectile.velocity.Y += 0.06f;
                    if (Projectile.velocity.Y > maxDown)
                        Projectile.velocity.Y -= 0.08f;
                }
            }

            Projectile.rotation = Projectile.velocity.X * 0.125f;
        }

        private void AnimateFrames()
        {
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= FrameCount)
                    Projectile.frame = 0;
            }
        }

        private NPC FindTarget(float maxDistance)
        {
            NPC target = null;
            float bestDistance = maxDistance;

            Player owner = Main.player[Projectile.owner];
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy(this))
                {
                    float d = Vector2.Distance(Projectile.Center, npc.Center);
                    if (d <= maxDistance)
                        return npc;
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                    continue;

                float d = Vector2.Distance(Projectile.Center, npc.Center);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    target = npc;
                }
            }

            return target;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.friendly = false;
            Projectile.velocity *= 0.15f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 6;
            Projectile.netUpdate = true;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.friendly;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Rectangle sourceRect = new Rectangle(
                Projectile.frame * FrameWidth,
                0,
                FrameWidth,
                FrameHeight
            );

            Vector2 origin = new Vector2(FrameWidth * 0.5f, FrameHeight * 0.5f);

            SpriteEffects effects = Projectile.spriteDirection == -1
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                sourceRect,
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );

            return false;
        }
    }
}