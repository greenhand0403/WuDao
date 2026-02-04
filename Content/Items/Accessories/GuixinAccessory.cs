using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    // 归心，回旋镖饰品，赋予回旋镖返程时速度翻倍的能力
    public class GuixinAccessory : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 80);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<BoomerangAccessoryPlayer>().Guixin = true;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe()
                .AddIngredient(ItemID.Bone, 10)
                .AddIngredient(ItemID.ManaCrystal, 5)
                .AddIngredient(ItemID.WoodenArrow, 99)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}