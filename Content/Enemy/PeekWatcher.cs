using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace WuDao.Content.Enemy
{
    public class PeekWatcher : ModNPC
    {
        // 贴图是 4 帧纵向排列
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
            // DisplayName.SetDefault("窥探者");
        }

        public override void SetDefaults()
        {
            // —— 数值 ——
            NPC.damage = 120;
            NPC.lifeMax = 900;
            NPC.defense = 20;
            NPC.knockBackResist = 0.36f;

            // —— 体型（比 Demon Eye 稍大一点点）——
            NPC.width = 74;
            NPC.height = 74;
            NPC.scale = 0.6f;
            // 贴图位置往上偏移一点，对齐视觉
            NPC.gfxOffY = -4f;

            // —— 其他基础属性 ——
            NPC.aiStyle = NPCAIStyleID.DemonEye; // 2
            AIType = NPCID.DemonEye;             // 复用 Demon Eye AI
            AnimationType = -1;                  // 我们自己控制帧

            NPC.noGravity = true;
            NPC.noTileCollide = false; // DemonEye 会在墙体附近处理移动，保持 false 更像原版
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 6, 0); // 可自行改

            // 可选：抗性/免疫等
            // NPC.buffImmune[BuffID.Confused] = true;

            // 可选：掉落、旗帜
            // Banner = Type;
            // BannerItem = ModContent.ItemType<PeekWatcherBannerItem>();
        }

        /// <summary>
        /// 你的贴图是“朝右姿态”，而 Terraria 默认大多 NPC 贴图是“朝左姿态”并通过 spriteDirection 翻转。
        /// 所以这里把 spriteDirection 反过来，让默认绘制逻辑翻转规则适配你的贴图方向。
        /// </summary>
        public override void AI()
        {
            // DemonEye AI 已经由 aiStyle/AIType 执行了，我们只修正朝向。
            NPC.spriteDirection = -NPC.direction;
        }

        /// <summary>
        /// 控制 4 帧动画：
        /// - life >= 50%：帧 0-1 循环（常态）
        /// - life < 50%：帧 2-3 循环（二形态）
        /// </summary>
        public override void FindFrame(int frameHeight)
        {
            bool phase2 = NPC.life < NPC.lifeMax * 0.5f;
            int startFrame = phase2 ? 2 : 0;
            int endFrame = startFrame + 1;

            // 动画速度：每 6 tick 换帧（你可以改成 5/7 等）
            NPC.frameCounter++;
            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;

                int current = NPC.frame.Y / frameHeight;
                current++;

                if (current < startFrame || current > endFrame)
                    current = startFrame;

                NPC.frame.Y = current * frameHeight;
            }
            else
            {
                // 第一次进来时确保在正确帧区间
                int current = NPC.frame.Y / frameHeight;
                if (current < startFrame || current > endFrame)
                    NPC.frame.Y = startFrame * frameHeight;
            }
        }

        /// <summary>
        /// 夜晚地表生成：OverworldHeight + 非白天 + 非城镇等
        /// </summary>
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Player p = spawnInfo.Player;

            // 只在夜晚
            if (Main.dayTime)
                return 0f;

            // 只在地表（OverworldHeight：地表/太空以下的常规地表层）
            if (!p.ZoneOverworldHeight)
                return 0f;

            // 常规限制：城镇/安全/事件你可按需求加
            if (spawnInfo.PlayerInTown)
                return 0f;

            // 用原版夜晚地表怪基准概率作为参考倍率
            // 你可以调这个系数：0.02 ~ 0.15 之间都常见
            return SpawnCondition.OverworldNightMonster.Chance * 0.08f;
        }

        // 可选：更合适的碰撞盒与贴图高度差较大时，通常会希望调一下 drawOffsetY 或 NPC.gfxOffY
        // public override void SetDefaults() { NPC.gfxOffY = ...; }

        // 可选：Bestiary（不写也不影响）
        // public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) { ... }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;

            int frameHeight = texture.Height / Main.npcFrameCount[Type];
            Rectangle frame = new Rectangle(0, NPC.frame.Y, texture.Width, frameHeight);

            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);

            // 把贴图往左挪，制造“碰撞箱更靠右”的视觉效果
            Vector2 drawOffset = new Vector2(-15f * NPC.direction, 0f);

            spriteBatch.Draw(
                texture,
                NPC.Center - screenPos + drawOffset,
                frame,
                drawColor,
                NPC.rotation,
                origin,
                NPC.scale,
                effects,
                0f
            );

            return false;
        }
    }
}