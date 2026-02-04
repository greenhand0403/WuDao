using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Mounts
{
    // 飞天马桶 坐骑
    public class FlyingToiletMount : ModMount
    {
        // 我们把玩家专属的“轮播状态”放到 _mountSpecificData 里（tML 官方建议做法）
        // 文档：SetMount/UpdateEffects/Draw 等钩子都允许这样做
        private sealed class ToiletCycleData
        {
            public int currentItemId = 4096; // 起始马桶
            public uint nextSwapTick;        // Main.GameUpdateCount 时间
        }

        // 4096..4127 共 32 个马桶
        private const int FirstToiletId = 4096;
        private const int LastToiletId = 4127;
        private const int CycleCount = LastToiletId - FirstToiletId + 1;
        private const int SwapIntervalTicks = 60 * 5; // 5 秒（60 tick = 1 秒）
                                                      // 放在类里（字段区）
        private const float VerticalAccel = 0.60f; // 上/下加速度
        private const float VerticalMaxSpeed = 8.00f; // 竖直最大速度
        private const float HoverBrakeFactor = 0.90f; // 悬停刹车

        public override string Texture => $"Terraria/Images/Item_{ItemID.TerraToilet}";
        public override void SetStaticDefaults()
        {
            Mount.MountData mountData = MountData;

            // 这里配置机动性 —— 参考灾厄嘉登飞椅“无限飞行、悬停、高速与高加速度”的手感
            mountData.buff = ModContent.BuffType<FlyingToiletBuff>();
            mountData.heightBoost = 16; // 玩家重心抬高，便于“坐着”的感觉
            mountData.fallDamage = 0f;

            // 速度与加速度：把 run/dash 调高；加速度高以获得“立刻起步/刹停”的手感
            mountData.runSpeed = 13.5f;   // 水平基础速度（数值偏大，实际显示约 50~60mph 量级）
            mountData.dashSpeed = 13.5f;   // 冲刺同速
            mountData.acceleration = 0.40f;  // 高加速度；(默认一般 ~0.08f 左右)

            // 跳跃只是备用（几乎不会用到，因为我们是悬停飞行）
            mountData.jumpHeight = 10;
            mountData.jumpSpeed = 8f;

            // 悬停 + 无限飞行
            mountData.usesHover = true;
            mountData.flightTimeMax = int.MaxValue / 2; // 实际上视为无限

            // 帧与偏移（我们自己画，不需要复杂帧）
            mountData.totalFrames = 1;
            mountData.playerYOffsets = new int[mountData.totalFrames];
            for (int i = 0; i < mountData.playerYOffsets.Length; i++) mountData.playerYOffsets[i] = 0;

            mountData.xOffset = 0;
            mountData.yOffset = 8;      // 稍微下沉，让人看起来“坐在马桶上”
            mountData.playerHeadOffset = 0;

            // 我们不使用内置 mount 贴图集；全部在 Draw 钩子里手动画“马桶道具贴图”
            // 所以这里让纹理为空即可（或指向 1x1 透明图）
            // mountData.backTexture = Asset<Texture2D>.Empty; 等，保持默认即可。
        }

        public override void SetMount(Player player, ref bool skipDust)
        {
            // 初始化玩家专属轮播数据
            player.mount._mountSpecificData = new ToiletCycleData
            {
                // currentItemId = FirstToiletId,
                currentItemId = FirstToiletId + Main.rand.Next(CycleCount),
                // nextSwapTick = Main.GameUpdateCount + SwapIntervalTicks
            };

            // 进入时的尘埃/特效可以自定义；此处略
        }

        public override void UpdateEffects(Player player)
        {
            // ====== 1) 轮播切换不同马桶 ======
            // if (player.mount?._mountSpecificData is ToiletCycleData data)
            // {
            //     if (Main.GameUpdateCount >= data.nextSwapTick)
            //     {
            //         int idx = (data.currentItemId - FirstToiletId + 1) % CycleCount;
            //         data.currentItemId = FirstToiletId + idx;
            //         data.nextSwapTick += (uint)SwapIntervalTicks;
            //     }
            // }

            // ====== 2) 水平手感（原样保留） ======
            if (!player.controlLeft && !player.controlRight)
            {
                player.velocity.X *= 0.90f;
            }

            // ====== 3) 竖直控制（关键修正） ======
            // 关闭重力，完全由我们控制上下
            player.gravity = 0f;

            bool ascend = player.controlUp || player.controlJump; // 上键或空格都向上
            bool descend = player.controlDown;

            if (ascend && !descend)
            {
                player.velocity.Y = MathHelper.Clamp(player.velocity.Y - VerticalAccel, -VerticalMaxSpeed, VerticalMaxSpeed);
            }
            else if (descend && !ascend)
            {
                player.velocity.Y = MathHelper.Clamp(player.velocity.Y + VerticalAccel, -VerticalMaxSpeed, VerticalMaxSpeed);
            }
            else
            {
                // 没按上下时，缓慢刹车到悬停
                player.velocity.Y *= HoverBrakeFactor;
                if (Math.Abs(player.velocity.Y) < 0.05f)
                    player.velocity.Y = 0f;
            }
        }

        public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
        {
            // 我们只有 1 帧，不用让系统切换帧
            return false; // 阻止默认帧行为
        }

        /// <summary>
        /// 关键：自定义绘制。我们接管“前景额外层”（drawType==3），
        /// 直接把当前马桶 Item 贴图画出来，位置基于引擎传入的 drawPosition。
        /// </summary>
        public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer,
            ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame,
            ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects,
            ref Vector2 drawOrigin, ref float drawScale, float shadow)
        {
            // 我们只在一个阶段执行一次，避免重复添加。选 3（front extra）仅为“只执行一次”的锚点。
            if (drawType == 3)
            {
                // 取当前轮播的马桶道具贴图
                int currentId = FirstToiletId;
                if (drawPlayer.mount?._mountSpecificData is ToiletCycleData data)
                    currentId = data.currentItemId;

                var asset = Main.Assets.Request<Texture2D>($"Images/Item_{currentId}", AssetRequestMode.AsyncLoad);
                if (asset?.IsLoaded == true)
                {
                    Texture2D itemTex = asset.Value;

                    // 以玩家的绘制坐标为基准做偏移：
                    // 1) “更靠后”：相对朝向的反方向挪动一些（dir=1朝右，-1朝左）
                    // 2) “更靠下”：整体下移若干像素
                    int dir = drawPlayer.direction; // 1 或 -1
                    Vector2 pos = drawPosition;

                    float backOffset = 8f;  // “更靠后”像素，按需要微调 8~14
                    float downOffset = 10f;  // “更靠下”像素，按需要微调 8~16

                    pos += new Vector2(-dir * backOffset, downOffset);

                    // 倒置重力时的轻微补偿（可选）
                    if (drawPlayer.gravDir == -1f)
                        pos.Y -= 8f;

                    Vector2 origin = new Vector2(itemTex.Width / 2f, itemTex.Height / 2f);
                    float scale = 1.2f;

                    var dd = new DrawData(itemTex, pos, null, drawColor, rotation, origin, scale, spriteEffects, 0);

                    // 核心：插到最前 => 此玩家的所有绘制项里，马桶永远在最底层，不会挡腿
                    playerDrawData.Insert(0, dd);
                }
            }

            // 始终禁止默认坐骑贴图（我们自己画）
            return false;
        }

    }
}
