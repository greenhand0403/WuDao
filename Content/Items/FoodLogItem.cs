using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Systems;
using WuDao.Content.Players;
using Terraria.Localization;

namespace WuDao.Content.Items
{
    /// <summary>
    /// 食谱：需要记录食物种类 ItemID.Sets.IsFood[Type] = true
    /// </summary>
    class FoodLogItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.rare = ItemRarityID.Green;
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

            // 未开启（未收藏）时：显示“说明版”Tooltip
            if (!cp.HasFoodLogItem)
            {
                tooltips.Clear();

                tooltips.Add(new TooltipLine(
                    Mod, "ItemName",
                    Language.GetTextValue("Mods.WuDao.Items.FoodLogItem.Tooltip.ClosedName")
                ));

                tooltips.Add(new TooltipLine(
                    Mod, "Title",
                    Language.GetTextValue("Mods.WuDao.Items.FoodLogItem.Tooltip.ClosedHint")
                ));

                return;
            }

            int totalFood = CuisineSystem.TotalFoodCount;

            tooltips.Add(new TooltipLine(
                Mod, "FoodEatenProgress",
                Language.GetTextValue(
                    "Mods.WuDao.Items.FoodLogItem.Tooltip.EatenProgress",
                    cp.FoodsEatenAll.Count, totalFood
                )
            ));

            tooltips.Add(new TooltipLine(
                Mod, "FoodDeliciousness",
                Language.GetTextValue(
                    "Mods.WuDao.Items.FoodLogItem.Tooltip.Deliciousness",
                    cp.Deliciousness
                )
            ));

            if (cp.SuggestedFoods6.Count > 0)
            {
                tooltips.Add(new TooltipLine(
                    Mod, "FoodHintsTitle",
                    Language.GetTextValue("Mods.WuDao.Items.FoodLogItem.Tooltip.SuggestionsTitle")
                ));

                foreach (int t in cp.SuggestedFoods6)
                {
                    tooltips.Add(new TooltipLine(
                        Mod, $"FoodHint_{t}",
                        $"[i:{t}] {Lang.GetItemNameValue(t)}"
                    ));

                    var rec = CuisineSystem.DescribeRecipeCompact(t);
                    tooltips.Add(new TooltipLine(Mod, $"FoodHintRecipe_{t}", rec));
                }
            }
        }

    }
}