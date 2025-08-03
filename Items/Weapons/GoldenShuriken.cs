using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Projectiles;
namespace WuDao.Items.Weapons
{
    public class GoldenShuriken : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Shuriken); // 克隆原版行为
            
            Item.damage = 14;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.shootSpeed = 9f;
            Item.crit = 6;
            Item.value = Item.buyPrice(silver: 15);
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<GoldenShurikenProjectile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(20)
                .AddIngredient(ItemID.GoldBar, 1)
                .AddIngredient(ItemID.IronBar, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
