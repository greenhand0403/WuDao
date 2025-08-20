using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace WuDao.Content.Items.Ammo
{
    // 日曜弹
    public class SolarBullet : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }
        public override void SetDefaults()
        {
            Item.shootSpeed = 2f;
            Item.shoot = Mod.Find<ModProjectile>("SolarBulletProjectile").Type;
            Item.damage = 15;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.ammo = AmmoID.Bullet;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(copper: 2);
            Item.rare = ItemRarityID.Purple;
            Item.DamageType = DamageClass.Ranged;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(300);
            recipe.AddIngredient(ItemID.FragmentSolar, 1);
            recipe.AddIngredient(ItemID.MusketBall, 300);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}