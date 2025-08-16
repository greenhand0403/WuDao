using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Common
{
    // 修改原版高尔夫球的行为
    public class GolfBallMeleeGlobal : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        // 命中计数
        private int hits;

        // 识别“是否为我们要接管的高尔夫球”
        private static bool IsGolfBall(int type)
        {
            // 把你在 Shoot 里用到的“各色高尔夫球 ProjectileID”都放进来
            return type == ProjectileID.DirtGolfBall
                || type == ProjectileID.GolfBallDyedRed
                || type == ProjectileID.GolfBallDyedOrange
                || type == ProjectileID.GolfBallDyedYellow
                || type == ProjectileID.GolfBallDyedGreen
                || type == ProjectileID.GolfBallDyedLimeGreen
                || type == ProjectileID.GolfBallDyedCyan
                || type == ProjectileID.GolfBallDyedBlue
                || type == ProjectileID.GolfBallDyedPurple
                || type == ProjectileID.GolfBallDyedViolet
                || type == ProjectileID.GolfBallDyedPink
                || type == ProjectileID.GolfBallDyedBrown
                || type == ProjectileID.GolfBallDyedBlack
                || type == ProjectileID.GolfBallDyedSkyBlue;
        }
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (!IsGolfBall(projectile.type))
                return;

            // 1) 伤害类型改为近战（吃近战加成）
            projectile.DamageType = DamageClass.Melee;

            // 2) 缩短寿命（默认 3600 tick≈60s，这里给个更短值，例如 240=4s）
            projectile.timeLeft = 240;

            // 3) 用本地免疫 + 命中计数来实现“最多打 5 次”
            //    （有些高尔夫球原本是非穿透、靠弹跳命中，这里用计数来控制）
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10; // 两次命中间隔，避免一帧多次
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!IsGolfBall(projectile.type))
                return;

            hits++;
            if (hits >= 5)
            {
                projectile.Kill(); // 第 5 次命中后立刻消失
            }
        }

        public override bool PreAI(Projectile projectile)
        {
            if (!IsGolfBall(projectile.type))
                return base.PreAI(projectile);

            // 保险：如果还没打 5 次但到了寿命也会自然消失（由 timeLeft 控制）
            return base.PreAI(projectile);
        }
    }
}
