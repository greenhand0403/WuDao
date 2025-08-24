using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Global.Systems;
using System.Collections.Generic;
// TODO: 改贴图
namespace WuDao.Content.Items
{
    public class Cookbook : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.CombatBook}";
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 26;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 1);
            Item.maxStack = 1;
            Item.consumable = false;         // 只是收藏品
        }

        public override void UpdateInventory(Player player)
        {
            player.GetModPlayer<CuisinePlayer>().HasCookbook = true; // ✅ 这是“菜谱”开关
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var p = Main.LocalPlayer;
            var cp = p.GetModPlayer<CuisinePlayer>();
            // 显示“已制作过/总数（食物池）”
            tooltips.Add(new TooltipLine(Mod, "CookbookMadeCount", $"已制作：{cp.CraftedFoodTypes.Count} / {CuisineSystem.FoodPool.Count} 种食物"));
            tooltips.Add(new TooltipLine(Mod, "CookbookCookingSkill", $"厨艺值：{cp.CookingSkill}"));
            CuisineSystem.GetTodayTwo(p, out int a, out int b);
            if (a > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "CookbookTodayA", $"今日推荐：[i:{a}] {Lang.GetItemNameValue(a)}"));
                var ra = CuisineSystem.DescribeRecipeCompact(a);
                if (!string.IsNullOrEmpty(ra))
                    tooltips.Add(new TooltipLine(Mod, "CookbookTodayARecipe", ra));
                else
                    tooltips.Add(new TooltipLine(Mod, "CookbookTodayARecipe", "无配方（可能为掉落/购买/宝匣/礼物袋）"));
            }

            if (b > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "CookbookTodayB", $"今日推荐：[i:{b}] {Lang.GetItemNameValue(b)}"));
                var rb = CuisineSystem.DescribeRecipeCompact(b);
                if (!string.IsNullOrEmpty(rb))
                    tooltips.Add(new TooltipLine(Mod, "CookbookTodayBRecipe", rb));
                else
                    tooltips.Add(new TooltipLine(Mod, "CookbookTodayBRecipe", "无配方（可能为掉落/购买/宝匣/礼物袋）"));
            }
        }
    }
}
