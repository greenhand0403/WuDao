using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Items.Weapons.Melee
{
    public class SteelBroadSword : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6f;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot; // 使用“持有型投射物”动画，由弹幕负责近战判定
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;

            Item.noUseGraphic = true; // 不显示物品本体，由 held projectile 绘制/判定
            Item.noMelee = true;      // 物品本体不产生近战判定，改由弹幕造成伤害

            Item.shoot = ModContent.ProjectileType<SteelBroadSwordProjectile>();

            Item.shootSpeed = 0f;     // held projectile 通常不需要速度

            Item.value = Item.buyPrice(silver: 4);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            // 接受任意木头、铜锭、铁锭
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup(RecipeGroupID.Wood, 2);
            recipe.AddIngredient(ItemID.CopperBar, 2);
            recipe.AddIngredient(ItemID.IronBar, 2);
            recipe.AddIngredient(ItemID.EnchantedSword, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
