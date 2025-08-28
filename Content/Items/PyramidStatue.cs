
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WuDao.Content.Tiles;
namespace WuDao.Content.Items
{
    /// <summary>
    /// 金字塔雕像：可放置雕像。
    /// 功效与“韧皮雕像”类似：附近玩家获得“金字塔守护”Buff，静止站立在实心块上时 +50% 免伤。
    /// </summary>
    public class PyramidStatue : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("金字塔雕像");
            // Tooltip.SetDefault("散发古老守护之力\n静止站立在实心块上时获得强力减伤");
        }

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 32;
            Item.maxStack = 1;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = Item.buyPrice(0, 1, 0);
            Item.rare = ItemRarityID.Orange;
            Item.createTile = ModContent.TileType<PyramidStatueTile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Sandstone, 20)
                .AddIngredient(ItemID.GoldBar, 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
