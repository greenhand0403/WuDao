using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace WuDao.Content.Projectiles.Melee
{
    /// <summary>
    /// 杀鲸霸拳 鲨鱼射弹（NPCID=65）
    /// </summary>
    public class SharkProjectile : ModProjectile
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.Shark}";
        public int FrameTicksPerFrame = 5;
        public override void SetStaticDefaults()
        {
            // 读取帧数，避免魔法数字
            Main.projFrames[Projectile.type] = Main.npcFrameCount[NPCID.Shark];
        }
        public override void SetDefaults()
        {
            Projectile.width = 100;  // 根据鲨鱼贴图大致调整
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
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
                if (Projectile.alpha > 0)
                    Projectile.alpha -= 20;
            }

            // 轻微旋转使朝向贴合速度
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用目标 NPC 的贴图 + 帧
            Texture2D tex = TextureAssets.Npc[NPCID.Shark].Value;

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
        // 击中造成 困惑减益
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 60);
        }
    }
}