using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using WuDao.Projectiles;

namespace WuDao.Items
{
    public class NebulaRocket : ModItem
    {
        public override void SetStaticDefaults()
        {
            AmmoID.Sets.IsSpecialist[Type] = true;
            AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher].Add(Type, ModContent.ProjectileType<NebulaRocketProjectile>());
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 16;
            Item.damage = 80;
            Item.knockBack = 4f;
            Item.consumable = true;
            Item.DamageType = DamageClass.Ranged;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(silver: 10);
            Item.rare = ItemRarityID.Purple;
            Item.ammo = AmmoID.Rocket;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(50);
            recipe.AddIngredient(ItemID.FragmentNebula, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}