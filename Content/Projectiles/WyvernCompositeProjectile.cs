using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures; // TextureAssets

namespace WuDao.Content.Projectiles
{
    /// <summary>
    /// 进阶：多贴图组成一个射弹（飞龙）。
    /// 飞龙（Wyvern）由多个 NPC 片段组成：头(87)、身(88/89/90)、腿(91)、尾(92)。
    /// 注意：腿段本身就包含左右两只爪（两个爪子），绘制顺序正确即可。
    /// 这里用一个射弹在 PreDraw 中依次绘制各段，使其沿速度方向排布，简单形成一条龙。
    /// </summary>
    public class WyvernCompositeProjectile : ModProjectile
    {
        // 段序列：头、身、身、腿、身、身、尾（可按需增减身段数量）
        private static readonly int[] SegmentNpcIds = new int[]
        {
            NPCID.WyvernHead,  // 87
            NPCID.WyvernBody2, // 89
            NPCID.WyvernLegs,  // 88
            NPCID.WyvernBody2, // 89
            NPCID.WyvernBody2, // 89
            NPCID.WyvernBody2, // 89
            NPCID.WyvernBody2, // 89
            NPCID.WyvernBody,  // 88
            NPCID.WyvernBody3, // 90：tModLoader 提供常量（1.4+）
            NPCID.WyvernLegs,  // 91（含两只爪）
            NPCID.WyvernTail   // 92
        };

        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            // 使用统一帧数推进：采用头的帧数作为参考，也可以为每段各自推进
            Main.projFrames[Projectile.type] = Math.Max(1, Main.npcFrameCount[NPCID.WyvernHead]);
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1; // 贯穿
            Projectile.timeLeft = 210;
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X >= 0 ? 1 : -1;
                Projectile.ai[0] = 1;
            }
            // 统一帧动画推进
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
        }

        private static Rectangle GetNPCFrameRect(int npcId, int frame)
        {
            Texture2D tex = TextureAssets.Npc[npcId].Value;
            int frames = Math.Max(1, Main.npcFrameCount[npcId]);
            int frameHeight = tex.Height / frames;
            return new Rectangle(0, frameHeight * (frame % frames), tex.Width, frameHeight);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float rotation = Projectile.velocity.ToRotation();
            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 段间距（根据贴图大小调整）：
            float spacing = 26f; // 每段中心到中心的距离

            // 从头到尾沿着速度方向的反向排布（尾巴在后）
            Vector2 headPos = Projectile.Center;

            for (int i = 0; i < SegmentNpcIds.Length; i++)
            {
                int npcId = SegmentNpcIds[i];
                Texture2D tex = TextureAssets.Npc[npcId].Value;
                // 各段可各自使用自己的帧数
                Rectangle frameRect = GetNPCFrameRect(npcId, Projectile.frame);
                Vector2 origin = new Vector2(frameRect.Width / 2f, frameRect.Height / 2f);

                // 计算该段的位置：头在射弹中心，其它段按 spacing 依次向速度反方向偏移
                Vector2 segWorldPos = headPos - (Projectile.velocity.SafeNormalize(Vector2.UnitX) * (i * spacing));
                Vector2 drawPos = segWorldPos - Main.screenPosition;

                // 使用环境光照
                Color drawColor = Lighting.GetColor((int)(segWorldPos.X / 16f), (int)(segWorldPos.Y / 16f));

                // 注意：腿段(91)自带两只爪，无需额外绘制
                Main.EntitySpriteDraw(tex, drawPos, frameRect, drawColor, rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f), origin, 1f, fx, 0);
            }
            return false;
        }
    }
}