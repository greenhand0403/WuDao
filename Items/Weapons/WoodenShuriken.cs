using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Projectiles;
namespace WuDao.Items.Weapons
{
    public class WoodenShuriken : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Shuriken); // 克隆原版行为
            
            Item.damage = 5;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.shootSpeed = 8f;
            Item.crit = 4;
            Item.value = Item.buyPrice(copper: 1);
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<WoodenShurikenProjectile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(20)
                .AddIngredient(ItemID.Wood, 2)
                .Register();
        }
    }
}
