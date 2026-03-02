using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Systems;
using System.Collections.Generic;
using Terraria.Localization;

namespace WuDao.Content.Items
{
    /// <summary>
    /// 菜谱
    /// </summary>
    public class Cookbook : ModItem
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
            if (!player.GetModPlayer<CuisinePlayer>().HasCookbook)
                player.GetModPlayer<CuisinePlayer>().HasCookbook = true; // ✅ 这是“菜谱”开关
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var p = Main.LocalPlayer;
            var cp = p.GetModPlayer<CuisinePlayer>();

            // 已制作：X / Y
            tooltips.Add(new TooltipLine(
                Mod, "CookbookMadeCount",
                Language.GetTextValue(
                    "Mods.WuDao.Items.Cookbook.Tooltip.MadeCount",
                    cp.CraftedFoodTypes.Count,
                    CuisineSystem.FoodPool.Count
                )
            ));

            // 厨艺值：X
            tooltips.Add(new TooltipLine(
                Mod, "CookbookCookingSkill",
                Language.GetTextValue(
                    "Mods.WuDao.Items.Cookbook.Tooltip.CookingSkill",
                    cp.CookingSkill
                )
            ));

            CuisineSystem.GetTodayTwo(p, out int a, out int b);

            if (a > 0)
            {
                string aText = $"[i:{a}] {Lang.GetItemNameValue(a)}";
                tooltips.Add(new TooltipLine(
                    Mod, "CookbookTodayA",
                    Language.GetTextValue("Mods.WuDao.Items.Cookbook.Tooltip.TodayRecommend", aText)
                ));

                var ra = CuisineSystem.DescribeRecipeCompact(a);
                tooltips.Add(new TooltipLine(
                    Mod, "CookbookTodayARecipe",
                    !string.IsNullOrEmpty(ra)
                        ? ra
                        : Language.GetTextValue("Mods.WuDao.Items.Cookbook.Tooltip.NoRecipeHint")
                ));
            }

            if (b > 0)
            {
                string bText = $"[i:{b}] {Lang.GetItemNameValue(b)}";
                tooltips.Add(new TooltipLine(
                    Mod, "CookbookTodayB",
                    Language.GetTextValue("Mods.WuDao.Items.Cookbook.Tooltip.TodayRecommend", bText)
                ));

                var rb = CuisineSystem.DescribeRecipeCompact(b);
                tooltips.Add(new TooltipLine(
                    Mod, "CookbookTodayBRecipe",
                    !string.IsNullOrEmpty(rb)
                        ? rb
                        : Language.GetTextValue("Mods.WuDao.Items.Cookbook.Tooltip.NoRecipeHint")
                ));
            }
        }
    }
}
