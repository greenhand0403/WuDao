using WuDao.Content.Systems;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Players;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using WuDao.Content.Items;

namespace WuDao.Content.Global
{
    public class CuisineGlobalItem : GlobalItem
    {
        // 更稳妥：制作瞬间直接扫描背包，而不是仅依赖 HasCookbook 标志
        private static bool HasCookbookNow(Player player)
        {
            for (int i = 0; i < player.inventory.Length; i++)
            {
                Item it = player.inventory[i];
                if (it?.ModItem is Cookbook) // 你的菜谱物品类名
                    return true;
            }
            return false;
        }
        public override void OnConsumeItem(Item item, Player player)
        {
            if (ItemID.Sets.IsFood[item.type])
            {
                CuisinePlayer p = player.GetModPlayer<CuisinePlayer>();
                if (p.FoodsEatenAll.Add(item.type))
                {
                    // 首次‘品尝’累加美味值
                    int bt = ContentSamples.ItemsByType[item.type].buffTime; // 帧
                    if (bt > 0) p.Deliciousness += bt;
                    CombatText.NewText(player.Hitbox, Color.Green, "品尝新食物");
                }
            }
        }

        public override void OnCreated(Item item, ItemCreationContext context)
        {
            var player = Main.LocalPlayer;
            var cp = player.GetModPlayer<CuisinePlayer>();

            bool hasCookbook = HasCookbookNow(player); // ✅ 以制作当帧的真实背包为准
            // —— 首次‘制作’累加厨艺值（与是否携带菜谱无关）——
            if (ItemID.Sets.IsFood[item.type] && cp.CraftedEverFoods.Add(item.type))
            {
                int bt = ContentSamples.ItemsByType[item.type].buffTime; // 单位：帧（60 = 1秒）
                if (bt > 0) cp.CookingSkill += bt;
            }
            // 仅当携带菜谱时才消耗‘首次双倍资格’与发奖励
            if (!hasCookbook || !ItemID.Sets.IsFood[item.type])
                return;

            // 今日两道（个人）
            CuisineSystem.GetTodayTwo(player, out int a, out int b);

            if (item.type == a || item.type == b)
            {
                player.QuickSpawnItem(player.GetSource_GiftOrReward(), item.type, item.stack);
                player.QuickSpawnItem(player.GetSource_GiftOrReward(), item.type, item.stack);
                CombatText.NewText(player.Hitbox, Color.Yellow, $"菜谱双倍奖励");
                cp.CraftedFoodTypes.Add(item.type);
                // —— 立刻补位（做掉第一道/第二道的规则在系统里处理）——
                CuisineSystem.OnCraftedAndRefresh(player, item.type);
            }
        }

    }
}