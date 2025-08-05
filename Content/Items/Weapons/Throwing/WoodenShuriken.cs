using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Items.Weapons.Throwing
{
    public class WoodenShuriken : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Shuriken); // 克隆原版行为
            Item.DamageType = DamageClass.Throwing;

            Item.damage = 5;
            Item.useTime = 14;
            Item.useAnimation = 14;
            
            Item.crit = 4;
            Item.value = Item.buyPrice(copper: 1);
            Item.rare = ItemRarityID.Green;

            Item.shoot = ModContent.ProjectileType<Projectiles.Throwing.WoodenShurikenProjectile>();
            Item.shootSpeed = 8f;
        }

        public override void AddRecipes()
        {
            CreateRecipe(30)
                .AddIngredient(ItemID.Wood, 2)
                .Register();
        }
    }
}
