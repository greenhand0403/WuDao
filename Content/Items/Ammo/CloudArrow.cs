using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Ranged;


namespace WuDao.Content.Items.Ammo
{
    // TODO: 重绘贴图 穿云箭 弹药
    public class CloudArrow : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 36;

            Item.damage = 10;
            Item.DamageType = DamageClass.Ranged;

            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(copper: 10);
            Item.shoot = ModContent.ProjectileType<CloudArrowProjectile>();
            Item.shootSpeed = 1.5f;
            Item.ammo = AmmoID.Arrow;
            Item.rare = ItemRarityID.Blue;
        }
        public override void AddRecipes()
        {
            CreateRecipe(30)
                .AddIngredient(ItemID.WoodenArrow, 30)
                .AddIngredient(ItemID.Cloud, 5)
                .AddIngredient(ItemID.SunplateBlock,5)
                .AddTile(TileID.SkyMill)
                .Register();
        }
    }
}
