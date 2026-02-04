using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using System;
using Terraria.ID;

namespace WuDao.Content.Projectiles.Melee
{
    /// <summary>
    /// 马射弹，复用三张物品贴图（ItemID=4785, 4786, 4787）来绘制一个“骏马形”的射弹。
    /// 通过生成时传入 ai0 选择贴图：0->4785，1->4786，2->4787。
    /// </summary>
    public class HorseItemVariantProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; // 隐藏占位，使用 PreDraw 绘制物品贴图
        public int FrameTicksPerFrame = 4;
        public int HorseType;
        // 加到类字段区域
        private int hoofTick;
        private static readonly Vector2[] HoofOffsets = new[] {
            new Vector2(-18f, 0f), // 左蹄：相对中心的水平偏移（像素）
            new Vector2(+18f, 0f), // 右蹄
        };

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 48;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 360;
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
            // 随机选择物品贴图
            HorseType = 161 + (int)Projectile.ai[0] % 3;
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
                if (Projectile.frame >= Main.projFrames[Projectile.type] - 1)
                    Projectile.frame = 10;
                if (Projectile.alpha > 0)
                    Projectile.alpha -= 20;
            }

            // 基础直线飞行 + 旋转对齐速度
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            // —— 在马蹄下生成“沙尘” —— //
            EmitHoofDust();
        }
        private void EmitHoofDust()
        {
            hoofTick++;

            // 低频持续尘。避免太多粒子：每 3 帧一次即可
            bool softBurst = (hoofTick % 3 == 0);
            // 脚落地的“重踩”帧（和你的跑步循环同步，10~15帧）
            bool hardStep = Projectile.frame == 11 || Projectile.frame == 13 || Projectile.frame == 15;

            if (!softBurst && !hardStep)
                return;

            // 马的朝向（-1/1），只镜像左右，不跟随旋转，以确保“尘在下方”
            int dir = Projectile.spriteDirection;

            // 以“碰撞箱底边”为基准更自然：Bottom 就是左上 + 高度，代表脚下那条线
            float baseY = Projectile.Bottom.Y - 2f;

            // 两只前后蹄：在底边附近各喷一点
            for (int i = 0; i < HoofOffsets.Length; i++)
            {
                float x = Projectile.Center.X + HoofOffsets[i].X * dir;
                Vector2 pos = new Vector2(x, baseY);

                // 尘的初速度：向上略喷 + 带一点水平速度，强/弱两档
                Vector2 vel = new Vector2(Projectile.velocity.X * 0.15f + Main.rand.NextFloat(-0.4f, 0.4f),
                                          hardStep ? Main.rand.NextFloat(-1.6f, -0.8f)
                                                   : Main.rand.NextFloat(-0.9f, -0.3f));

                // 选一个土/沙系尘，更像蹄下扬尘；你也可以用 Dirt/Sand/Smoke
                int dustId = DustID.Sand; // 可换 DustID.Dirt、DustID.Smoke 看效果
                var d = Dust.NewDustPerfect(pos, dustId, vel, 100,
                                            new Color(210, 180, 140) * (hardStep ? 0.8f : 0.6f),
                                            hardStep ? 1.1f : 0.85f);
                d.noGravity = false;   // 让它自然落下
                d.fadeIn = 0f;
                d.velocity += Projectile.velocity * 0.05f;
            }
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

            Main.EntitySpriteDraw(tex, drawPos, src, Color.White, Projectile.rotation, origin, 1f, fx, 0);
            return false;
        }
    }
}