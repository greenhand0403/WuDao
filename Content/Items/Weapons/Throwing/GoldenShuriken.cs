using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Items.Weapons.Throwing
{
    public class GoldenShuriken : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Shuriken); // 克隆原版行为
            Item.DamageType = DamageClass.Throwing;
            
            Item.damage = 14;
            Item.useTime = 16;
            Item.useAnimation = 16;


            Item.crit = 2;
            Item.value = Item.buyPrice(silver: 15);
            Item.rare = ItemRarityID.Green;

            Item.shoot = ModContent.ProjectileType<Projectiles.Throwing.GoldenShurikenProjectile>();
            Item.shootSpeed = 9f;
        }

        public override void AddRecipes()
        {
            CreateRecipe(30)
                .AddIngredient(ItemID.GoldBar, 1)
                .AddIngredient(ItemID.IronBar, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
