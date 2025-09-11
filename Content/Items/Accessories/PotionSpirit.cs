using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    public class PotionSpirit : ModItem
    {
        // TODO: 补贴图
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("药水之灵");
        //     Tooltip.SetDefault("治疗药水冷却就绪时，遭受致命伤害前会自动饮用背包中排序靠前的治疗药水或蘑菇\n" +
        //                        "（若仍然致命且你持有永生之酒且冷却就绪，则会自动饮用永生之酒保命）");
        // }
        public override string Texture => $"Terraria/Images/Item_{ItemID.SharkToothNecklace}";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PotionPlayer>().hasPotionSpirit = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofLight, 10)
                .AddIngredient(ItemID.Bottle, 1)
                .AddIngredient(ItemID.GreaterHealingPotion, 5)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}
