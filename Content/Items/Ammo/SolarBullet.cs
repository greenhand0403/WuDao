using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace WuDao.Content.Items.Ammo
{
    public class SolarBullet:ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount=99;
        }
        public override void SetDefaults()
        {
            Item.damage = 17;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(copper: 10);
            Item.rare = ItemRarityID.Purple;
            Item.shoot = Mod.Find<ModProjectile>("SolarBulletProjectile").Type;
            Item.shootSpeed = 3.5f;
            Item.ammo = AmmoID.Bullet;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(100);
            recipe.AddIngredient(ItemID.FragmentSolar, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}