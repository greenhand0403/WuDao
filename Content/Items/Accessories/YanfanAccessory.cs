using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    /// <summary>
    /// 燕返，回旋镖饰品，赋予回旋镖返程伤害翻倍能力
    /// </summary>
    public class YanfanAccessory : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(silver: 50);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<BoomerangAccessoryPlayer>().Yanfan = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Trimarang)
                .AddIngredient(ItemID.Feather, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}