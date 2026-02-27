using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    /// <summary>
    /// 近视眼镜 饰品
    /// </summary>
    public class NearsightedGlasses : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 3, 0, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var gp = player.GetModPlayer<NearsightedPlayer>();
            gp.Nearsighted = true;
            gp.ShowRangeRings = !hideVisual;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Goggles)
                .AddIngredient(ItemID.HellstoneBar, 2)
                .AddTile(TileID.Hellforge)
                .Register();
        }
    }
}
