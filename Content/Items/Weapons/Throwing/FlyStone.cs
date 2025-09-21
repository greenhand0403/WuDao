using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Items.Weapons.Throwing
{
    // ========= 1) 飞蝗石（物品） =========
    public class FlyStone : BaseThrowingItem
    {
        public override int BaseProjectileType => ModContent.ProjectileType<FlyStoneProjectile>();
        public override int ProjectileAIMode => 1; // 1 = 飞石（受重力、有击退）


        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 12;
            Item.useTime = 12;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;


            Item.DamageType = DamageClass.Throwing; // 若你使用了自定义 Throwing，请保持一致
            Item.damage = 6;
            Item.knockBack = 3.5f; // 飞石有击退
            Item.value = Item.buyPrice(silver: 1);
            Item.rare = ItemRarityID.White;
            Item.shoot = BaseProjectileType;
            Item.shootSpeed = 10f;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.UseSound = SoundID.Item1;
        }


        public override void AddRecipes()
        {
            CreateRecipe(33)
            .AddIngredient(ItemID.StoneBlock, 10)
            .AddTile(TileID.HeavyWorkBench)
            .Register();
        }
    }
}