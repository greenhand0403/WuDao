using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Ranged

{
    public class CloudArrowProjectile : ModProjectile
    {
        // —— 可调参数（椭圆版） ——
        private const int RingPeriod = 8;        // 每几帧喷一圈
        private const int RingDustCount = 16;    // 每圈粒子数量
        private const float RingStartRadius = 4f;
        private const float RingOutSpeed = 2.0f;    // 尘粒向外扩散速度

        public override void SetStaticDefaults()
        {
            // 可选：显示弹体历史用于拖影（不需要就删掉）
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = 1;// 让它像箭一样受重力/碰撞（也可以自写 AI）
            AIType = ProjectileID.WoodenArrowFriendly;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 360;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.MaxUpdates = 2;
        }

        public override void AI()
        {
            // 对齐箭头朝向
            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 控制间隔，比如每 8 帧生成一次环
            if (++Projectile.localAI[0] >= RingPeriod)
            {
                Projectile.localAI[0] = 0f;
                float verticalScale = 2.4f; // Y 方向拉伸比例（>1 为纵向椭圆）

                for (int i = 0; i < RingDustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / RingDustCount;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle) * verticalScale) * RingStartRadius;

                    int dustIndex = Dust.NewDust(
                        Projectile.Center - new Vector2(3f) + offset,
                        0, 0,
                        DustID.Cloud, // 可换成你想要的 Dust
                        0f, 0f,
                        100, default,
                        0.9f + Main.rand.NextFloat(0.15f)
                    );
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].velocity = offset.SafeNormalize(Vector2.UnitY) * 1.2f;
                    Main.dust[dustIndex].fadeIn = 0.9f + Main.rand.NextFloat(0.2f);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 结束时再放一圈更大的环，呼应“爆散”
            int bigCount = (int)(RingDustCount * 1.3f);
            for (int i = 0; i < bigCount; i++)
            {
                float angle = MathHelper.TwoPi * i / bigCount;
                Vector2 radial = angle.ToRotationVector2();
                Vector2 pos = Projectile.Center + radial * (RingStartRadius + 4f);
                Vector2 vel = radial * (RingOutSpeed + 1.2f);

                Dust d = Dust.NewDustPerfect(pos, DustID.Cloud, vel, 90, default, 1.2f);
                d.noGravity = true;
            }

            // 可选：音效
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }
    }
}
