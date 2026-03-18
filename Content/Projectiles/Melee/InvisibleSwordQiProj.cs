using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Melee
{
    public class InvisibleSwordQiProj : ModProjectile
    {
        // 这张图现在是“遮罩贴图”，不是最终可见贴图
        public override string Texture => "WuDao/Assets/InvisibleSwordQiMask";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            // 完全不走普通可见精灵
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // 轻微脉动，让遮罩边缘更“活”
            Projectile.scale = 1f + 0.04f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 18f + Projectile.whoAmI);

            // 给 shader 额外一点局部强度参考
            Projectile.localAI[0] += 0.08f;

            // 很淡的照明，别太像能量弹
            Lighting.AddLight(Projectile.Center, 0.05f, 0.05f, 0.06f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;

            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 start = Projectile.Center - dir * 58f;
            Vector2 end = Projectile.Center + dir * 58f;

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                start,
                end,
                18f * Projectile.scale,
                ref collisionPoint
            );
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 不绘制普通 sprite，视觉完全交给后处理系统
            return false;
        }
    }
}