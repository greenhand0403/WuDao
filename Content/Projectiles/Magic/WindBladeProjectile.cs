using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Magic
{
    public class WindBladeProjectile : ModProjectile
    {
        // 复用 Arkhalis 弹幕的首帧贴图
        // tML 支持直接引用原版：Projectile_<ID>
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Arkhalis}";

        public override void SetStaticDefaults()
        {
            // 只取用第一帧：Arkhalis 原弹幕是多帧，我们把帧数锁到 1
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30; // 贴图是拉刀状，不需要太大 hitbox
            Projectile.height = 30;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2; // 穿透 2 个目标
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;

            // 初速在 Item.Shoot 里给，这里只定基础行为
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {
            // 旋转对齐速度方向 + 少量拉风效果
            if (Projectile.velocity.Length() > 0.01f)
                Projectile.rotation = Projectile.velocity.ToRotation();

            // 微弱追踪鼠标，避免出生点太偏导致看起来“跑偏”
            Vector2 toMouse = (Main.MouseWorld - Projectile.Center);
            float steerStrength = 0.12f; // 轻微转向力，避免过度自导
            Vector2 desiredVel = toMouse.SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, steerStrength);

            // 风感尘埃（纯视觉）
            if (Main.rand.NextBool(3))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Cloud,
                    0f, 0f, 150, default, 0.9f);
                Main.dust[d].velocity *= 0.3f;
                Main.dust[d].noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 轻微反弹一次感（降低速度）
            if (Projectile.penetrate > 1)
            {
                if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X * 0.6f;
                if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y * 0.6f;
                Projectile.penetrate--;
                return false;
            }
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // 消散尘埃
            for (int i = 0; i < 6; i++)
            {
                int d = Dust.NewDust(Projectile.Center - new Vector2(10), 20, 20, DustID.Cloud, 0f, 0f, 160, default, 1.1f);
                Main.dust[d].velocity *= 1.2f;
                Main.dust[d].noGravity = true;
            }
            // 轻声
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);
        }
    }
}
