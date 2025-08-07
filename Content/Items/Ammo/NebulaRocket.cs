using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace WuDao.Content.Items.Ammo
{
    public class NebulaRocket : ModItem
    {
        public override void SetStaticDefaults()
        {
            AmmoID.Sets.IsSpecialist[Type] = true;
            AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher].Add(Type, ModContent.ProjectileType<Projectiles.Ranged.NebulaRocketProjectile>());
        }
        public override void SetDefaults()
        {
            Item.damage = 70;
            Item.width = 24;
            Item.height = 16;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.ammo = AmmoID.Rocket;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(silver: 1);
            Item.DamageType = DamageClass.Ranged;
            Item.rare = ItemRarityID.Purple;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(100);
            recipe.AddIngredient(ItemID.RocketIII, 100);
            recipe.AddIngredient(ItemID.FragmentNebula, 1);
            recipe.AddIngredient(ItemID.ShroomiteBar, 1);
            recipe.AddTile(TileID.Autohammer);
            recipe.Register();
        }
    }
}