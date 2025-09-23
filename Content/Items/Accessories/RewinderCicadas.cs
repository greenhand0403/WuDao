using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

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

        // public override void AddRecipes()
        // {
        //     CreateRecipe()
        //         .AddIngredient(ItemID.Firefly, 3)
        //         .AddIngredient(ItemID.SoulofMight, 3)
        //         .AddIngredient(ItemID.SoulofSight, 3)
        //         .AddIngredient(ItemID.SoulofFright, 3)
        //         .AddIngredient(ItemID.HallowedBar, 5)
        //         .AddIngredient(ItemID.LifeCrystal, 5)
        //         .AddTile(TileID.MythrilAnvil)
        //         .Register();
        // }
    }
}
