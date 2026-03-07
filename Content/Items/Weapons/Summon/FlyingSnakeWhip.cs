using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Items.Weapons.Summon
{
    public class FlyingSnakeWhip : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<FlyingSnakeWhipProjectile>(), 120, 3, 8);
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 0, 50);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LunarTabletFragment, 2)
                .AddIngredient(ItemID.LihzahrdPowerCell)
                .AddIngredient(ItemID.FlyingSnakeBanner)
                .AddTile(TileID.LihzahrdFurnace)
                .Register();
        }
        public override bool MeleePrefix()
        {
            return true;
        }
        public override void HoldItem(Player player)
        {
            player.GetModPlayer<FlyingSnakeWhipPlayer>().HoldingFlyingSnakeWhip = true;
        }
    }
}
