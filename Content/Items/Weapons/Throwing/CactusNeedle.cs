using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Items.Weapons.Throwing
{
    // ========= 2) 仙人掌飞针（物品） =========
    public class CactusNeedle : BaseThrowingItem
    {
        public override int BaseProjectileType => ModContent.ProjectileType<CactusNeedleProjectile>();
        public override int ProjectileAIMode => 0; // 0 = 飞针（无重力、无击退）


        public override void SetDefaults()
        {
            Item.width = 8;
            Item.height = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;


            Item.DamageType = DamageClass.Throwing;
            Item.damage = 4;
            Item.knockBack = 0f; // 飞针无击退（基类也会在 AI 中强制为 0）
            Item.value = Item.sellPrice(copper: 8);
            Item.rare = ItemRarityID.White;
            Item.shoot = BaseProjectileType;
            Item.shootSpeed = 8f;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.UseSound = SoundID.Item1;
        }


        public override void AddRecipes()
        {
            CreateRecipe(50)
            .AddIngredient(ItemID.Cactus, 10)
            .AddTile(TileID.WorkBenches)
            .Register();
        }
    }
}