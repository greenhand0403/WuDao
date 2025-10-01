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
            NPCID.WyvernHead,
            NPCID.WyvernBody,
            NPCID.WyvernLegs,
            NPCID.WyvernBody,
            NPCID.WyvernBody,
            NPCID.WyvernLegs,
            NPCID.WyvernBody2,
            NPCID.WyvernBody3,
            NPCID.WyvernTail
        };
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetStaticDefaults()
        {
            // 放宽离屏裁剪，合成“长龙”需要更宽的范围
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 800;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10; // 10帧同一NPC只吃一次
        }
        public override void SetDefaults()
        {
            Projectile.width = 74;
            Projectile.height = 74;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1; // 贯穿
            Projectile.timeLeft = 300;
            Projectile.aiStyle = 0;
            Projectile.hostile = false;
            Projectile.light = 0.5f;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X >= 0 ? 1 : -1;
                Projectile.ai[0] = 1;
            }

            // Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 方向/位置与 PreDraw 同步
            Vector2 dir = Projectile.velocity.LengthSquared() > 0.001f
                ? Vector2.Normalize(Projectile.velocity)
                : new Vector2(Projectile.direction, 0f);
            Vector2 step = -dir * 44f; // 跟 PreDraw 的 spacing 保持一致
            Vector2 segPos = Projectile.Center;

            float hitRadius = Projectile.width / 2; // 每段的“判定粗细”，按你的贴图大小调
            for (int i = 0; i < SegmentNpcIds.Length; i++)
            {
                // AABB
                if (Collision.CheckAABBvAABBCollision(targetHitbox.Center(), targetHitbox.Size(), segPos, new Vector2(hitRadius, hitRadius)))
                    return true;
                segPos += step;
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 方向/旋转（速度为0时兜底）
            Vector2 dir = Projectile.velocity.LengthSquared() > 0.001f
                ? Vector2.Normalize(Projectile.velocity)
                : new Vector2(Projectile.direction, 0f);
            float rot = dir.ToRotation();
            SpriteEffects fx = Main.LocalPlayer.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 以“头”为起点，后续各段按 spacing 反向排布
            float spacing = 44f;
            Vector2 step = -dir * spacing;
            Vector2 segPos = Projectile.Top;

            for (int i = 0; i < SegmentNpcIds.Length; i++)
            {
                int npcId = SegmentNpcIds[i];

                // 确保贴图已加载（避免 1×1 占位贴图）
                Main.instance.LoadNPC(npcId);
                Texture2D tex = TextureAssets.Npc[npcId].Value;

                Rectangle frame = new Rectangle(0, 0, tex.Width, tex.Height);
                Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);

                Vector2 drawPos = segPos - Main.screenPosition;
                // 注意：左向时加 PI，贴图朝向才一致
                float drawRot = rot + MathHelper.PiOver2;
                Main.EntitySpriteDraw(tex, drawPos, frame, Color.White, drawRot, origin, 1f, fx, 0);

                segPos += step; // 下一段往后排
            }
            return false; // 禁用默认绘制
        }

    }
}