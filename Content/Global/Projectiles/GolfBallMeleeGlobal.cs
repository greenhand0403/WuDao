using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items.Weapons.Melee;

namespace WuDao.Content.Global.Projectiles
{
    // 修改原版高尔夫球的行为
    public class GolfBallMeleeGlobal : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        // 给“彩虹杆生成的高尔夫球”打标记
        private bool fromRainbowClub;
        // 命中计数
        private int hits;
        private int lifeTicks; // 我们自己的寿命计数器
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
            if (source is EntitySource_ItemUse_WithAmmo isrc && isrc.Item?.type == ModContent.ItemType<RainBowGolfClubs>())
            {
                fromRainbowClub = true;
                // 1) 伤害类型改为近战（吃近战加成）
                projectile.DamageType = DamageClass.Melee;
                // 3) 用本地免疫 + 命中计数来实现“最多打 5 次”
                lifeTicks = 360;
                //    （有些高尔夫球原本是非穿透、靠弹跳命中，这里用计数来控制）
                projectile.usesLocalNPCImmunity = true;
                projectile.localNPCHitCooldown = 10; // 两次命中间隔，避免一帧多次
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!fromRainbowClub) return;

            hits++;
            if (hits >= 5)
            {
                projectile.Kill(); // 第 5 次命中后立刻消失
            }
        }

        public override void PostAI(Projectile projectile)
        {
            if (!fromRainbowClub) return;

            // 每帧在 AI 之后递减寿命；到点强制 Kill
            if (lifeTicks > 0)
            {
                lifeTicks--;
                if (lifeTicks <= 0)
                {
                    projectile.Kill();
                    return;
                }
            }

            // （可选）把原版被拉高的 timeLeft 往下压，避免网络端显示差异
            if (projectile.timeLeft > lifeTicks)
                projectile.timeLeft = lifeTicks;
        }
    }
}
