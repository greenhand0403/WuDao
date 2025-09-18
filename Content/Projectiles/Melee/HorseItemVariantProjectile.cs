using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using System;

namespace WuDao.Content.Projectiles.Melee
{
    /// <summary>
    /// 复用三张物品贴图（ItemID=4785, 4786, 4787）来绘制一个“骏马形”的射弹。
    /// 通过生成时传入 ai0 选择贴图：0->4785，1->4786，2->4787。
    /// </summary>
    public class HorseItemVariantProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; // 隐藏占位，使用 PreDraw 绘制物品贴图
        public int FrameTicksPerFrame = 5;
        public int HorseType;
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 48;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
        }
        public override void SetStaticDefaults()
        {
            // 读取帧数，避免魔法数字
            Main.projFrames[Projectile.type] = 16;
        }
        public override void OnSpawn(IEntitySource source)
        {
            HorseType = 161 + Main.rand.Next(0, 3);
            // 设定朝向
            Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X >= 0 ? 1 : -1;
        }

        public override void AI()
        {
            // 帧动画推进
            if (++Projectile.frameCounter >= FrameTicksPerFrame)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type]-1)
                    Projectile.frame = 10;
                if (Projectile.alpha > 0)
                    Projectile.alpha -= 20;
            }

            // 基础直线飞行 + 旋转对齐速度
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Extra[HorseType].Value;

            int frames = Main.projFrames[Projectile.type];
            int frameHeight = tex.Height / frames;
            Rectangle src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            // 以贴图中心为原点绘制
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 让光照影响颜色
            // Color drawColor = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));

            Main.EntitySpriteDraw(tex, drawPos, src, Color.White, Projectile.rotation, origin, 1f, fx, 0);
            return false;
        }
    }
}