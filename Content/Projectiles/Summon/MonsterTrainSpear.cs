using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Summon
{
    public class MonsterTrainSpear : ModProjectile
    {
        // 继续复用原版矛贴图（10x42，尖端朝上）
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.PygmySpear}";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 42;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;

            Projectile.tileCollide = false;  // ✅ 穿墙
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 90;        // 飞行寿命可调
            Projectile.extraUpdates = 1;     // 更顺滑
        }

        public override void AI()
        {
            // 贴图尖端竖直向上（rotation=0时朝上）
            // velocity.ToRotation() 是“朝右”为0，所以要 +Pi/2
            if (Projectile.velocity.LengthSquared() > 0.01f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;
    }
}