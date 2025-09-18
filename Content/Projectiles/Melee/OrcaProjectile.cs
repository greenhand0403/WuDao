using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace WuDao.Content.Projectiles.Melee
{
    /// <summary>
    /// 杀鲸霸拳 鲸鱼射弹（NPCID=65）
    /// </summary>
    public class OrcaProjectile : ModProjectile
    {
        public int FrameTicksPerFrame = 5;
        public float MinSpeed = 10f;
        public override void SetStaticDefaults()
        {
            // 读取帧数，避免魔法数字
            Main.projFrames[Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 40;
            Projectile.aiStyle = 0; // 自定义 AI
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180; // 3 秒
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.spriteDirection = -1;
            Projectile.penetrate = -1;
            Projectile.alpha = 200;
        }
        public override void AI()
        {
            // 简单直线飞行：保持速度方向，受少量重力可选
            if (Projectile.ai[0] == 0)
            {
                // 首帧可设定朝向
                Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X >= 0f ? 1 : -1;
                Projectile.ai[0] = 1;
            }

            // 帧动画推进
            if (++Projectile.frameCounter >= FrameTicksPerFrame)
            {
                Projectile.frameCounter = 0;
                // if (++Projectile.frame >= Main.projFrames[Projectile.type])
                //     Projectile.frame = 0;
                if (Projectile.alpha > 0)
                    Projectile.alpha -= 20;
            }

            // 轻微旋转使朝向贴合速度
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用自身的贴图
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

            int frames = Main.projFrames[Projectile.type];
            int frameHeight = tex.Height / frames;
            Rectangle src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            Vector2 origin = new Vector2(src.Width / 2f, src.Height / 2f);

            // 翻转
            SpriteEffects fx = (Projectile.velocity.X >= 0f)
                ? SpriteEffects.FlipVertically
                : SpriteEffects.None;

            // 绘制位置（注意：Projectile.Center 是世界坐标中心点）
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 让光照影响颜色
            // Color drawColor = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));

            Main.EntitySpriteDraw(tex, drawPos, src, lightColor, Projectile.rotation, origin, 1f, fx, 0);
            return false; // 我们已手动绘制
        }
        // 击中造成 流血减益
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.BrokenArmor, 60);
        }
    }
}