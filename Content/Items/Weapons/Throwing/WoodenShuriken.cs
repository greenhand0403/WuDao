using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Items.Weapons.Throwing
{
    // TODO: 未继承飞镖基类的木飞镖，需要统一
    public class WoodenShuriken : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Shuriken); // 克隆原版行为
            Item.shootSpeed = 8f;
            Item.shoot = ModContent.ProjectileType<WoodenShurikenProjectile>();
            Item.damage = 6;
            Item.useAnimation = 14;
            Item.useTime = 14;
            Item.autoReuse = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(copper: 5);
            Item.DamageType = DamageClass.Throwing;
        }

        public override void AddRecipes()
        {
            CreateRecipe(30)
                .AddIngredient(ItemID.Wood, 2)
                .Register();
        }
    }
}
