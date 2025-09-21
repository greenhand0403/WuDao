using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Projectiles.Throwing
{
    public class CactusNeedleProjectile : BaseThrowingProjectile
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
            Projectile.timeLeft = 70;           // 存活70tick
            Projectile.ignoreWater = true;       // 不被水减速
            Projectile.tileCollide = true;       // 碰砖块就消失（也可改成 false 直穿）
            Projectile.MaxUpdates = 2;
            Projectile.alpha = 255;              // 初始完全透明 -> 淡入
            Projectile.rotation = 0f;            // 旋转在AI里更新
        }

        public override void AI()
        {
            // 先执行基类（会根据 ai[0] 区分为“针”模式）
            base.AI();
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
            // 为飞针加一点点尾迹尘土
            if (Main.rand.NextBool(8))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grass);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.2f;
            }
        }
        public override void ImpactEffects(Vector2 position, Vector2 velocity)
        {
            for (int i = 0; i < 8; i++)
            {
                int d = Dust.NewDust(position, Projectile.width, Projectile.height, DustID.Grass, velocity.X * 0.15f, velocity.Y * 0.15f);
                Main.dust[d].noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            // 20% 中毒 + 20% 流血
            if (Main.rand.NextBool(5))
                target.AddBuff(BuffID.Poisoned, 120);
            if (Main.rand.NextBool(5))
                target.AddBuff(BuffID.Bleeding, 120);
        }
    }
}