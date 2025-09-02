using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items.Weapons.Melee;

namespace WuDao.Content.Global.Projectiles
{
    public class GolfBallMeleeGlobal : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        // ai[1] = 1 标记来自彩虹杆； ai[0] = 剩余寿命 tick
        private const int MaxLife = 360;

        private static bool IsGolfBall(int type) =>
            type == ProjectileID.DirtGolfBall
            || type == ProjectileID.GolfBallDyedRed
            || type == ProjectileID.GolfBallDyedOrange
            || type == ProjectileID.GolfBallDyedYellow
            || type == ProjectileID.GolfBallDyedGreen          // 绿色
            || type == ProjectileID.GolfBallDyedLimeGreen      // 黄绿色
            || type == ProjectileID.GolfBallDyedCyan
            || type == ProjectileID.GolfBallDyedSkyBlue
            || type == ProjectileID.GolfBallDyedBlue
            || type == ProjectileID.GolfBallDyedPurple
            || type == ProjectileID.GolfBallDyedViolet
            || type == ProjectileID.GolfBallDyedPink
            || type == ProjectileID.GolfBallDyedBrown
            || type == ProjectileID.GolfBallDyedBlack;

        private static bool IsFromRainbowClub(Projectile projectile, IEntitySource source)
        {
            // 放宽：ItemUse / ItemUse_WithAmmo 都算；另外用持有物兜底
            bool bySource =
                source is EntitySource_ItemUse_WithAmmo isrc1 && isrc1.Item?.type == ModContent.ItemType<RainBowGolfClubs>()
             || source is EntitySource_ItemUse isrc2 && isrc2.Item?.type == ModContent.ItemType<RainBowGolfClubs>();

            if (bySource) return true;

            // 兜底：看发射者当帧的持有物（有时 source 不带 item）
            int owner = projectile.owner;
            if (owner >= 0 && owner < Main.maxPlayers)
            {
                var p = Main.player[owner];
                if (p?.HeldItem?.type == ModContent.ItemType<RainBowGolfClubs>())
                    return true;
            }
            return false;
        }

        private static void InitRainbowGolfBall(Projectile projectile)
        {
            // 标记 + 初始化（同步字段）
            projectile.ai[1] = 1f;
            projectile.ai[0] = MaxLife;

            projectile.DamageType = DamageClass.Melee;

            // 用引擎穿透更稳（若你要自数命中，改为 ai[2] 亦可）
            if (projectile.penetrate <= 0 || projectile.penetrate > 10)
                projectile.penetrate = 5;

            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;

            projectile.netUpdate = true;
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (!IsGolfBall(projectile.type)) return;

            if (IsFromRainbowClub(projectile, source))
            {
                InitRainbowGolfBall(projectile);
            }
        }

        public override void PostAI(Projectile projectile)
        {
            // —— 补课式初始化：有些生成路径拿不到 item/source，这里兜底一次 ——
            if (projectile.ai[1] != 1f && IsGolfBall(projectile.type))
            {
                // 只在前几帧尝试补一次，避免误伤别的来源的高尔夫玩法
                if (projectile.timeLeft > 1800 - 5) // 高尔夫类原版通常长寿命；“刚生成”的一个粗略判据
                {
                    InitRainbowGolfBall(projectile);
                }
            }

            if (projectile.ai[1] != 1f) return; // 仍未标记则不接管

            // —— 服务端权威地递减寿命，并在到期时 Kill ——
            if (Main.netMode != NetmodeID.MultiplayerClient) // 服务器/单机
            {
                float remain = projectile.ai[0];
                if (remain > 0f)
                {
                    remain -= 1f;
                    projectile.ai[0] = remain;

                    if (remain <= 0f)
                    {
                        projectile.Kill();
                        return;
                    }
                }
            }

            // —— 反向钳制，覆盖原版每帧刷新 timeLeft 的行为 ——
            int remainInt = (int)projectile.ai[0];
            if (remainInt > 0 && projectile.timeLeft > remainInt)
                projectile.timeLeft = remainInt;

            // —— 防止“可见但不伤害” ——
            if (!projectile.friendly)
                projectile.friendly = true;
        }
    }
}
