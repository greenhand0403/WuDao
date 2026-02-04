using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    /// <summary>
    /// 药水之灵饰品
    /// </summary>
    public class PotionSpirit : ModItem
    {
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
