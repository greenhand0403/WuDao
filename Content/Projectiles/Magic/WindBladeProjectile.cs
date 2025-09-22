using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;

namespace WuDao.Content.Projectiles.Magic
{
    public class WindBladeProjectile : ModProjectile
    {
        // 复用 Arkhalis 贴图（多帧）
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Arkhalis}";
        private const int curFrame = 2;
        public override void SetStaticDefaults()
        {
            // 用 Arkhalis 的帧数作为我们的帧数
            Main.projFrames[Type] = Main.projFrames[ProjectileID.Arkhalis];
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;    // 最多穿透 2 个敌怪
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 碰墙要生效
            Projectile.aiStyle = 0;        // 自定义 AI（无重力）
            Projectile.alpha = 255;        // 淡入
            Projectile.light = 0.2f;
        }
        
        public override void AI()
        {
            // ai[0]：滞留计时器（可能从负数开始，见 Shoot）
            // localAI[0]/[1]：生成瞬间的目标鼠标坐标
            int delay = 12;           // 滞留帧数
            float launchSpeed = 12f;  // 发射速度

            // 淡入
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 20;
                if (Projectile.alpha < 0) Projectile.alpha = 0;
            }

            // 帧动画驱动（使用 Arkhalis 所有帧）
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            { // 动画速度可调
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Type])
                    Projectile.frame = 0;
            }

            // 尚在滞留阶段：计数并吸收速度
            if (Projectile.ai[0] < delay)
            {
                Projectile.ai[0]++;
                Projectile.velocity *= 0.9f;

                // 预瞄朝向（让刀锋指向目标）
                Vector2 preAim = new Vector2(Projectile.localAI[0], Projectile.localAI[1]) - Projectile.Center;
                if (preAim.LengthSquared() > 0.001f)
                    Projectile.rotation = preAim.ToRotation();
                return;
            }

            // 发射（只做一次）
            if (Projectile.localAI[2] == 0f)
            {
                Vector2 target = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
                if (target == Vector2.Zero) target = Main.MouseWorld; // 兜底

                Vector2 dir = target - Projectile.Center;
                if (dir.LengthSquared() < 0.001f) dir = Vector2.UnitX;
                dir.Normalize();

                Projectile.velocity = dir * launchSpeed;
                Projectile.localAI[2] = 1f;
                Projectile.netUpdate = true;
            }

            // 飞行中朝向速度方向
            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.rotation = Projectile.velocity.ToRotation();
        }

        // ——碰到物块立刻消失——
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return true; // true = 杀死弹幕；不做反弹
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                int d = Dust.NewDust(Projectile.Center - new Vector2(10), 20, 20, DustID.Cloud, 0f, 0f, 160, default, 1.1f);
                Main.dust[d].velocity *= 1.2f;
                Main.dust[d].noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);
        }

        // 手动绘制：从 Arkhalis 贴图裁切“当前帧”
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[ProjectileID.Arkhalis].Value;
            int totalFrames = Main.projFrames[ProjectileID.Arkhalis];
            if (totalFrames <= 0) totalFrames = 1;

            int frameHeight = tex.Height / totalFrames;
            // 可以只取某一帧作为射弹贴图
            int frameY = frameHeight * (curFrame % totalFrames);
            Rectangle src = new Rectangle(0, frameY, tex.Width, frameHeight);

            Vector2 origin = src.Size() * 0.5f;
            Color drawColor = Projectile.GetAlpha(lightColor);

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                src,
                drawColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            return false; // 我们已完成绘制
        }
    }
}
