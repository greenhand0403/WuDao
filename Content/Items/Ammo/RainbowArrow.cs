using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Ranged;

namespace WuDao.Content.Items.Ammo
{
    // 彩虹箭
    public class RainbowArrow : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 36;
            Item.damage = 12;
            Item.DamageType = DamageClass.Ranged;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(copper: 20);
            Item.shoot = ModContent.ProjectileType<RainbowArrowProjectile>();
            Item.shootSpeed = 3.5f;
            Item.ammo = AmmoID.Arrow;
            Item.rare = ItemRarityID.Orange;
        }

        public override void AddRecipes()
        {
            CreateRecipe(50)
                .AddIngredient(ItemID.WoodenArrow, 50)
                .AddIngredient(ItemID.FallenStar, 1)
                .AddIngredient(ItemID.PixieDust, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}