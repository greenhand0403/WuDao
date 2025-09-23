using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using WuDao.Content.Items;

namespace WuDao.Content.Players
{
    public class StaticFishCatchPlayer : ModPlayer
    {
        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
        {
            // 仅水中钓鱼；如果你也想支持熔岩/蜂蜜可去掉这行
            if (attempt.inLava || attempt.inHoney)
                return;

            // 固定 7% 概率，不看任何渔力/药水/饵料等
            if (Main.rand.NextFloat() < 0.07f)
            {
                // 把掉落直接替换成“静止游鱼”
                itemDrop = ModContent.ItemType<TimeStopItem>();
            }
        }
    }
}