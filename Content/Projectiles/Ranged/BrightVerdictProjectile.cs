using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using System;

namespace WuDao.Content.Projectiles.Ranged
{
    // 十字星射弹 改了碰撞箱
    public class BrightVerdictProjectile : ModProjectile
    {
        private bool _burstFired;
        const int BoostFrames = 6;      // 前 6 帧加速
        const float BoostFactor = 1.2f; // 每帧×1.06 ≈ 1.42 倍（与发射端夹速共同作用）
        const float MaxFlightSpeed = 22f;// 绝对速度上限（双保险）
        private ref float BoostTimer => ref Projectile.localAI[0];
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;    // 保留一些穿透；可按需改
            Projectile.light = 0.3f;     // 自带淡光
            Projectile.usesIDStaticNPCImmunity = true;   // 同类型共享冷却
            Projectile.idStaticNPCHitCooldown = 10;      // 10 帧共享冷却（按需调小/调大）
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {
            // 方向
            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            // —— 起步短促加速（不想要就删）——
            if (BoostTimer < BoostFrames)
            {
                Projectile.velocity *= BoostFactor;
                float spd = Projectile.velocity.Length();
                if (spd > MaxFlightSpeed) Projectile.velocity *= (MaxFlightSpeed / spd);
                BoostTimer++;
            }
            // 纯白/暖金尾焰（无紫无黑）
            for (int i = 0; i < 2; i++)
            {
                int d1 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemDiamond);
                var a = Main.dust[d1];
                a.noGravity = true;
                a.velocity *= 0.25f;
                a.scale = 1.0f + Main.rand.NextFloat(0.2f);
                a.noLight = false;
            }
            if (Main.rand.NextBool(3))
            {
                int d2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FireworkFountain_Yellow);
                var b = Main.dust[d2];
                b.noGravity = true;
                b.velocity *= 0.3f;
                b.scale = 0.9f + Main.rand.NextFloat(0.2f);
                b.noLight = false;
            }

            // 柔白光
            Lighting.AddLight(Projectile.Center, 0.9f, 0.9f, 0.85f);
        }

        // —— 命中敌怪：触发圣光十字星爆裂 ——
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            TryBurstOnce(Projectile.Center, Projectile.velocity);
        }

        // —— 命中方块：也触发（并让弹体正常死亡/反弹按默认）——
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            TryBurstOnce(Projectile.Center, oldVelocity);
            return base.OnTileCollide(oldVelocity); // 使用原本的碰撞处理
        }

        // —— 兜底：弹体死亡时若还没触发，也再来一次（避免极端情况漏效果）——
        public override void OnKill(int timeLeft)
        {
            // 仅在没有刚刚命中的情况下补一次（简单做法：总是补一次，视觉更爽）
            TryBurstOnce(Projectile.Center, Projectile.velocity);
        }
        private void TryBurstOnce(Vector2 center, Vector2 baseVelocity)
        {
            if (_burstFired)
            {
                return;
            }
            _burstFired = true;
            if (Projectile.owner == Main.myPlayer)
            {
                SpawnHolyCrossBurst(center, baseVelocity);
            }
        }
        private void SpawnHolyCrossBurst(Vector2 center, Vector2 baseVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.25f }, center);

            // float baseAngle = baseVelocity.ToRotation();
            // if (float.IsNaN(baseAngle)) baseAngle = -MathHelper.PiOver2;

            float b = 42f;                 // 横向半宽（越小越瘦越尖）
            int duration = 1;              // 瞬发
            float[] offs = { 0f, MathHelper.PiOver2, MathHelper.Pi, MathHelper.Pi * 3f / 2f };

            foreach (float off in offs)
            {
                int p = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    center, Vector2.Zero,
                    ModContent.ProjectileType<HolyKiteBurstHitbox>(),
                    Projectile.damage, Projectile.knockBack, Projectile.owner
                );
                if (p >= 0 && p < Main.maxProjectiles)
                {
                    var proj = Main.projectile[p];
                    proj.Center = center;
                    proj.ai[0] = off;  // 不随着碰撞面的方向 baseAngle + 
                    proj.localAI[0] = b;           // 横向半宽；纵向 a 自动由公式算
                    proj.timeLeft = duration;

                    proj.friendly = true;
                    proj.hostile = false;
                    proj.DamageType = Projectile.DamageType;
                }
            }
        }
    }
}
