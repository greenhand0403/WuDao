using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Items.Weapons.Summon
{
    public class FlyingSnakeWhip : ModItem
    {
        // public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(FlyingSnakeWhipDebuff.TagDamage);
        public override string Texture => $"Terraria/Images/Item_{ItemID.BoneWhip}";
        public override void SetDefaults()
        {
            // This method quickly sets the whip's properties.
            // Mouse over to see its parameters.
            Item.DefaultToWhip(ModContent.ProjectileType<FlyingSnakeWhipProjectile>(), 20, 2, 4);
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 0, 50);
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
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
