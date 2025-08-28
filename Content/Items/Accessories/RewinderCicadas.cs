using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Cooldowns;

namespace WuDao.Content.Items.Accessories
{
    public class RewinderCicadas : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RewinderCicadasPlayer>().equipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LifeCrystal, 1)
                .AddIngredient(ItemID.SoulofLight, 10)
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}
