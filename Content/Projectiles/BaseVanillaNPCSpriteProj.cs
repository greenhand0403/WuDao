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
    /// 通用：使用原版 NPC 贴图播放帧动画的基础射弹。
    /// - 通过 PreDraw 使用 TextureAssets.Npc[npcId] 绘制
    /// - 按 Main.npcFrameCount[npcId] 自动循环帧
    /// - 通过 timeLeft 自动消失
    /// </summary>
    public abstract class BaseVanillaNPCSpriteProj : ModProjectile
    {
        public abstract int TargetNPCId { get; }
        public virtual int FrameTicksPerFrame => 5; // 每多少 tick 切一次帧
        public virtual int InitialTimeLeft => 120;  // 存活时间（2 秒）
        public virtual float Speed => 12f;          // 初速度（由发射端设置也可）

        protected int FrameCount;

        public override string Texture => "Terraria/Images/Projectile_0"; // 隐藏占位

        public override void SetStaticDefaults()
        {
            // 读取帧数，避免魔法数字
            FrameCount = Main.npcFrameCount[TargetNPCId];
        }

        public override void SetDefaults()
        {
            Projectile.width = 32; // 实际碰撞箱，可按需调整
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = InitialTimeLeft;
            Projectile.aiStyle = 0; // 自定义 AI
            Projectile.hide = true; // 不绘制自身贴图，改为手动绘制 NPC 贴图
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
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= FrameTicksPerFrame)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= FrameCount)
                    Projectile.frame = 0;
            }

            // 轻微旋转使朝向贴合速度
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
        }

        /// <summary>
        /// 计算给定 NPC 帧矩形
        /// </summary>
        protected Rectangle GetNPCFrameRect(int npcId, int frame)
        {
            Texture2D tex = TextureAssets.Npc[npcId].Value;
            int frames = Math.Max(1, Main.npcFrameCount[npcId]);
            int frameHeight = tex.Height / frames;
            return new Rectangle(0, frameHeight * (frame % frames), tex.Width, frameHeight);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用目标 NPC 的贴图 + 帧
            Texture2D tex = TextureAssets.Npc[TargetNPCId].Value;
            Rectangle frameRect = GetNPCFrameRect(TargetNPCId, Projectile.frame);
            Vector2 origin = new Vector2(frameRect.Width / 2f, frameRect.Height / 2f);

            // 翻转
            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 绘制位置（注意：Projectile.Center 是世界坐标中心点）
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 让光照影响颜色
            Color drawColor = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));

            Main.EntitySpriteDraw(tex, drawPos, frameRect, drawColor, Projectile.rotation, origin, 1f, fx, 0);
            return false; // 我们已手动绘制
        }
    }

    /// <summary>
    /// ① 鲨鱼射弹（NPCID=65）
    /// </summary>
    public class SharkProjectile : BaseVanillaNPCSpriteProj
    {
        public override int TargetNPCId => NPCID.Shark; // 65
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 56;  // 根据鲨鱼贴图大致调整
            Projectile.height = 28;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180; // 3 秒
        }
    }

    /// <summary>
    /// ② 独角兽射弹（NPCID=86）
    /// </summary>
    public class UnicornProjectile : BaseVanillaNPCSpriteProj
    {
        public override int TargetNPCId => NPCID.Unicorn; // 86
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 60;
            Projectile.height = 44;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
        }
    }

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
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1; // 贯穿
            Projectile.timeLeft = 210;
            Projectile.aiStyle = 0;
            Projectile.hide = true; // 手动绘制
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

// --- Added: HorseItemVariantProjectile ---
namespace WuDao.Content.Projectiles
{
    /// <summary>
    /// 复用三张物品贴图（ItemID=4785, 4786, 4787）来绘制一个“骏马形”的射弹。
    /// 通过生成时传入 ai0 选择贴图：0->4785，1->4786，2->4787。
    /// </summary>
    public class HorseItemVariantProjectile : ModProjectile
    {
        private static readonly int[] VariantItemIds = new int[] { 4785, 4786, 4787 };
        public override string Texture => "Terraria/Images/Projectile_0"; // 隐藏占位，使用 PreDraw 绘制物品贴图

        public override void SetDefaults()
        {
            Projectile.width = 64;   // 粗略碰撞箱，必要时可在 OnSpawn 根据贴图重新设定
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = 0;
            Projectile.hide = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 设定朝向
            Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X >= 0 ? 1 : -1;
        }

        public override void AI()
        {
            // 基础直线飞行 + 旋转对齐速度
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
        }

        private int GetSelectedItemId()
        {
            int idx = (int)MathHelper.Clamp((float)Math.Round(Projectile.ai[0]), 0f, VariantItemIds.Length - 1);
            return VariantItemIds[idx];
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int itemId = GetSelectedItemId();
            Texture2D tex = TextureAssets.Item[itemId].Value;

            // 以贴图中心为原点绘制
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 让光照影响颜色
            Color drawColor = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));

            Main.EntitySpriteDraw(tex, drawPos, null, drawColor, Projectile.rotation, origin, 1f, fx, 0);
            return false;
        }

        // 可选：与地形碰撞后消失
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }
    }
}

/*
使用方法：
1) 在任意发射端（物品/技能/NPC）里生成射弹：
   Projectile.NewProjectile(source, player.Center, direction * speed, ModContent.ProjectileType<SharkProjectile>(), damage, knockback, player.whoAmI);
   或 UnicornProjectile / WyvernCompositeProjectile。

2) 如果你想按原版的朝向规则（例如某些 NPC 帧有偏移），可以在 PreDraw 中根据具体贴图做 origin/offset 微调。

3) 如果希望与地形碰撞后释放特效，请在 OnTileCollide 里处理并 return true 让其消失，或修改 timeLeft。

4) 需要更平滑的“蛇形”效果，可以在 AI 里记录一个历史轨迹数组（如 List<Vector2> pastPositions），然后各段跟随上一段的位置与旋转。
   本例为最小实现，重点演示如何复用 NPC 贴图与帧动画 + 组合绘制飞龙（含腿段的两个爪）。
*/
