using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Ranged
{
    // 终极爆弹（拜月邪教徒后解锁，4次暴击后下一次发射）
    // 击中首个敌人后：在敌人身后扇形方向造成90%溅射；未命中则飞行到时限后在前方扇形爆炸90%伤害
    public class TheOutlawUltimateBomb : ModProjectile
    {
        private bool _detonated;
        // 你自己可随时改动/做成 ModConfig
        public const float SplashHalfAngleDeg = 120f;   // 溅射半角（度）
        public const float SplashRange = 360f;   // 溅射距离（像素）
        public const float SplashDamageMult = 0.90f;  // 溅射伤害倍率

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MiniNukeGrenadeI}";
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("终极爆弹");
        // }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 120;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // 对齐飞行方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90);
            Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.8f);
            if (Main.rand.NextBool(3))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 120, default, 1.1f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 未命中任何敌人时的自动爆炸
            if (!_detonated) ConeExplode(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 90f, (int)(Projectile.damage * 0.9f));
            _detonated = true;
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            if (!_detonated)
            {
                ConeExplode(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), SplashHalfAngleDeg, (int)(Projectile.damage * SplashDamageMult));
                _detonated = true;
            }
            for (int i = 0; i < 24; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4), 100, default, 1.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (_detonated) return;
            // 击中首个敌人后：对“敌人身后”的扇形进行溅射（120%）
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 origin = target.Center + dir * 8f; // 从目标中心略偏后
            ConeExplode(origin, dir, SplashHalfAngleDeg, (int)(Projectile.damage * SplashDamageMult), ignoreFirst: target.whoAmI);

            _detonated = true;
        }

        private void ConeExplode(Vector2 origin, Vector2 forward, float halfAngleDeg, int damage, int ignoreFirst = -1)
        {
            if (Projectile.owner != Main.myPlayer) return;

            float halfRad = MathHelper.ToRadians(halfAngleDeg);

            // 溅射距离：建议抽成常量/配置
            float range = SplashRange;

            // 预归一 + 容错
            Vector2 fwd = forward;
            if (fwd.LengthSquared() < 0.001f) fwd = Vector2.UnitX;
            else fwd.Normalize();

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.life <= 0 || npc.dontTakeDamage) continue;
                if (i == ignoreFirst) continue;

                // 允许攻击训练假人；其它友方单位跳过
                bool isDummy = npc.type == NPCID.TargetDummy;
                if (npc.friendly && !isDummy) continue;
                if (npc.townNPC) continue;

                // 计算“到扇形中心”的向量
                Vector2 to = npc.Center - origin;
                float dist = to.Length();
                if (dist <= 1f) dist = 1f;
                Vector2 dir = to / dist;

                // —— 距离判定：把 NPC 的“半径”计入（让大个子、靠边缘的也容易吃到）——
                // 用 AABB 的等效半径近似
                float npcRadius = (npc.width + npc.height) * 0.25f; // 粗略半径
                if (dist - npcRadius > range) continue;

                // —— 角度判定：对大体型放松一点边缘误差（角度阈值 + epsilon）——
                float ang = MathF.Acos(MathHelper.Clamp(Vector2.Dot(fwd, dir), -1f, 1f));
                // 距离越近，允许的角度越宽一点点（避免贴脸偏轴没命中）
                float nearBonus = MathHelper.Lerp(0.15f, 0f, MathHelper.Clamp((dist - npcRadius) / range, 0f, 1f)); // 近处 +~8.6°
                if (ang > (halfRad + nearBonus)) continue;

                // 命中：不击退
                var hitInfo = new NPC.HitInfo
                {
                    Damage = damage,
                    Knockback = 0f,
                    HitDirection = (to.X >= 0 ? 1 : -1),
                    Crit = false
                };
                npc.StrikeNPC(hitInfo, fromNet: false);

                // 简单特效（可删）
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.PurpleCrystalShard);
            }

            // 视觉：可以多一点粒子增强“打到了很多目标”的反馈
            for (int j = 0; j < 50; j++)
            {
                float t = Main.rand.NextFloat(-halfRad, halfRad);
                Vector2 vel = fwd.RotatedBy(t) * Main.rand.NextFloat(7f, 12f);
                Dust.NewDustPerfect(origin, DustID.PurpleCrystalShard, vel, 120, default, Main.rand.NextFloat(1.2f, 1.8f));
            }
        }
    }
}
