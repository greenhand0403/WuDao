using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Global.Systems;
using WuDao.Content.Players;

namespace WuDao.Content.Items
{
    /// <summary>
    /// 需要记录食物种类 ItemID.Sets.IsFood[Type] = true
    /// </summary>
    class FoodLogItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 1);
            Item.maxStack = 1;
            Item.consumable = false;         // 只是收藏品
        }
        public override void UpdateInventory(Player player)
        {
            player.GetModPlayer<CuisinePlayer>().HasFoodLogItem = true;
            // 把“收藏(⭐)”作为‘开启食谱提示’的条件
            var cp = player.GetModPlayer<CuisinePlayer>();
            bool on = Item.favorited; // 只有收藏才算“开启”
            if (on)
            {
                cp.HasFoodLogItem = true;
            }
            else
            {
                cp.HasFoodLogItem = false;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var cp = Main.LocalPlayer.GetModPlayer<CuisinePlayer>();
            if (!cp.HasFoodLogItem)
            {
                tooltips.Clear();
                tooltips.Add(new TooltipLine(Mod, "Title", "收藏后，会显示未品尝的食物建议"));
                return;
            }

            // 全部食物（包括不可合成 + 模组食物）：扫描 ItemLoader.ItemCount + IsFood 标记
            int totalFood = CuisineSystem.TotalFoodCount;

            tooltips.Add(new TooltipLine(Mod, "FoodEatenProgress", $"已吃过：{cp.FoodsEatenAll.Count} / {totalFood} 种食物"));
            tooltips.Add(new TooltipLine(Mod, "FoodDeliciousness", $"美味值：{cp.Deliciousness}"));
            if (cp.HasFoodLogItem && cp.SuggestedFoods6.Count > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "FoodHintsTitle", "快去品尝这些食物吧："));
                foreach (int t in cp.SuggestedFoods6)
                {
                    // tooltips.Add(new TooltipLine(Mod, $"· [i:{t}] FoodHint_{t}", Lang.GetItemNameValue(t)));
                    tooltips.Add(new TooltipLine(Mod, $"FoodHint_{t}", $"[i:{t}] {Lang.GetItemNameValue(t)}"));
                    var rec = CuisineSystem.DescribeRecipeCompact(t);
                    // if (!string.IsNullOrEmpty(rec))
                        tooltips.Add(new TooltipLine(Mod, $"FoodHintRecipe_{t}", rec));
                    // else
                    //     tooltips.Add(new TooltipLine(Mod, $"FoodHintRecipe_{t}", CuisineSystem.DescribeAcquisition(t)));
                }
            }
        }

    }
}