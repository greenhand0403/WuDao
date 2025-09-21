using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;

namespace WuDao.Content.Projectiles.Throwing
{
    // 基类射弹：包含两种 AI 行为
    // ai[0] = 0 -> 飞针（不受重力、无击退）
    // ai[0] = 1 -> 飞石（受重力、有击退）
    public abstract class BaseThrowingProjectile : ModProjectile
    {
        // 可覆盖参数
        protected virtual float GravityPerTick => 0.15f; // 飞石模式使用
        protected virtual float XDragWhenFast => 0.15f; // X 方向阻力

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 0;

            // 默认朝向
            Projectile.rotation = 0f;
        }


        public override void AI()
        {
            // 更新朝向使其面向速度方向
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;


            int mode = (int)Projectile.ai[0];
            if (mode == 0)
            {
                // 飞针行为：不受重力、无击退（确保击退为0）
                Projectile.knockBack = 0f;
                // 不更改 velocity.Y
            }
            else
            {
                // 飞石行为：受重力且有击退
                // X 方向轻微阻力（仅当水平速度很快时）
                if (Math.Abs(Projectile.velocity.X) > 1.5f)
                    Projectile.velocity.X += Projectile.velocity.X > 0 ? -XDragWhenFast : XDragWhenFast;


                // 重力
                Projectile.velocity.Y += GravityPerTick;
            }


            // 通用：轻微尘埃
            if (Main.rand.NextBool(12))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.15f;
            }
        }

        // 命中时的通用特效（子类可 override）
        public virtual void ImpactEffects(Vector2 position, Vector2 velocity)
        {
            for (int i = 0; i < 8; i++)
            {
                int d = Dust.NewDust(position, Projectile.width, Projectile.height, DustID.Stone, velocity.X * 0.1f, velocity.Y * 0.1f);
                Main.dust[d].noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ImpactEffects(Projectile.position, Projectile.velocity);
        }

        public override void OnKill(int timeLeft)
        {
            ImpactEffects(Projectile.position, Projectile.velocity);
        }
    }
}
