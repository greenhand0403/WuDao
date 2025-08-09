using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Projectiles.Throwing
{
    public class CactusNeedleProjectile : ModProjectile
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Throwing/CactusNeedle";
        // 可调参数
        private const int FadeInTime = 10;   // 出生后多少tick内淡入
        private const int FadeOutTime = 10;  // 即将消失前多少tick开始淡出

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;

            Projectile.aiStyle = 0;              // 自定义AI，保持直线，不受重力
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Throwing;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;           // 存活3秒
            Projectile.ignoreWater = true;       // 不被水减速
            Projectile.tileCollide = true;       // 碰砖块就消失（也可改成 false 直穿）
            Projectile.extraUpdates = 0;         // 如需更丝滑可设为1

            Projectile.alpha = 255;              // 初始完全透明 -> 淡入
            Projectile.rotation = 0f;            // 旋转在AI里更新
        }

        public override void AI()
        {
            // 1) 维持朝向：沿着速度方向指向
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 2) 不受重力：不做任何Y速度修改（保持直飞）
            //    不使用 aiStyle=1（箭）以避免重力和空气阻力

            // 3) 淡入
            if (Projectile.timeLeft > FadeOutTime)
            {
                // 出生到开始淡出这段，只做淡入
                if (Projectile.localAI[0] < FadeInTime)
                {
                    Projectile.localAI[0]++;
                    // 线性从 255 -> 0
                    float t = Projectile.localAI[0] / FadeInTime;
                    Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, t);
                    if (Projectile.alpha < 0) Projectile.alpha = 0;
                }
                else
                {
                    Projectile.alpha = 0; // 完全可见
                }
            }
            else
            {
                // 4) 淡出（最后 FadeOutTime tick）
                int ticksIntoFadeOut = FadeOutTime - Projectile.timeLeft;
                float t = ticksIntoFadeOut / (float)FadeOutTime;
                Projectile.alpha = (int)MathHelper.Lerp(0f, 255f, t);
                if (Projectile.alpha > 255) Projectile.alpha = 255;
            }

            // 5) 轻微的飞行尘粒（不想太夸张）
            if (Main.rand.NextBool(12))
            {
                int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Grass, 0f, 0f, 100, default, 0.9f);
                Main.dust[dustId].velocity = Projectile.velocity * 0.1f;
                Main.dust[dustId].noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 20% 中毒 + 20% 流血
            if (Main.rand.NextBool(5))
                target.AddBuff(BuffID.Poisoned, 120);
            if (Main.rand.NextBool(5))
                target.AddBuff(BuffID.Bleeding, 120);

            // 命中粒子（少量、快速散开）
            for (int i = 0; i < 6; i++)
            {
                Vector2 v = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 2.0f);
                int dustId = Dust.NewDust(target.Hitbox.TopLeft(), target.Hitbox.Width, target.Hitbox.Height,
                    DustID.Grass, v.X, v.Y, 150, default, 1.1f);
                Main.dust[dustId].noGravity = true;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // 若未来有敌对版本也能复用：给玩家同样少量粒子
            for (int i = 0; i < 4; i++)
            {
                int dustId = Dust.NewDust(target.Hitbox.TopLeft(), target.Hitbox.Width, target.Hitbox.Height,
                    DustID.Grass, 0f, 0f, 150, default, 1.0f);
                Main.dust[dustId].noGravity = true;
                Main.dust[dustId].velocity *= 1.2f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            // 消失时也来一点尘粒但数量更少
            for (int i = 0; i < 4; i++)
            {
                int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Grass, 0f, 0f, 120, default, 0.9f);
                Main.dust[dustId].noGravity = true;
            }
            // 可选：SoundEngine.PlaySound(SoundID.Grass, Projectile.Center);
        }
    }
}