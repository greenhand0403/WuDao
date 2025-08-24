using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Items.Weapons.Throwing
{
    // TODO: 仙人掌飞针 改贴图 需要抽象出飞针基类
    public class CactusNeedle : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 8;
            Item.height = 8;

            Item.consumable = true;
            Item.maxStack = Item.CommonMaxStack;

            Item.useStyle = ItemUseStyleID.Swing; // 类似手里剑
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.autoReuse = true;

            Item.noUseGraphic = true; // 使用时不显示物品贴图（只显示弹幕）
            Item.noMelee = true;      // 不造成近战伤害

            Item.DamageType = DamageClass.Throwing; // 如需兼容，可改为 DamageClass.Ranged
            Item.damage = 4;

            Item.rare = ItemRarityID.Green;
            Item.value = 1;

            Item.shoot = ModContent.ProjectileType<CactusNeedleProjectile>();
            Item.shootSpeed = 8f;
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