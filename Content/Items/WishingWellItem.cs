// Content/Items/WishingWellItem.cs
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Tiles;

// TODO: 改贴图
namespace WuDao.Content.Items
{
    /// <summary>
    /// 复制宝藏袋或装备 或者 生成BOSS
    /// </summary>
    public class WishingWellItem : ModItem
    {
        // 物品贴图直接用原版抽取机
        public override string Texture => "Terraria/Images/Item_" + ItemID.Extractinator;

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange;

            Item.createTile = ModContent.TileType<WishingWellTile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Extractinator, 1)
                .AddIngredient(ItemID.FallenStar, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
