using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures; // PlayerDrawSet
using WuDao.Content.Mounts;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Players
{
    /// <summary>
    /// 飞天马桶：仅在绘制阶段，把穿着的腿部帧切到“坐姿腿（向前伸）”，不改玩家状态，避免 ESC 等导致坠落。
    /// </summary>
    public sealed class FlyingToiletPosePlayer : ModPlayer
    {
        private const int frameIndex = 11;
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            var p = drawInfo.drawPlayer;
            if (!p.mount.Active || p.mount.Type != ModContent.MountType<FlyingToiletMount>())
                return;

            // 强制腿部使用“坐姿腿”帧
            p.legFrameCounter = 0.0;
            p.legFrame.Y = p.legFrame.Height * frameIndex;
            // 让脚几乎处于静止（避免跑步腿帧覆盖）
            p.legFrame.X = 0;
        }
    }
}
