using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Projectiles.Throwing;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Items.Weapons.Throwing
{
    public class GemPouch : BaseThrowingItem
    {
        // 指定默认射弹为 GemProjectile
        public override int BaseProjectileType => ModContent.ProjectileType<GemProjectile>();
        // 我想让宝石表现为飞石（受重力）
        public override int ProjectileAIMode => 1;

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.DamageType = DamageClass.Throwing;
            Item.damage = 8;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ItemRarityID.Blue;

            Item.shoot = BaseProjectileType;
            Item.shootSpeed = 10f;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true; // 你可以改成 false 做无限射击
        }

        // 重写 Shoot 使每次发射时随机选择外观（我们通过 projectile.frame 来传递外观索引）
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int projIndex = Projectile.NewProjectileDirect(source, position, velocity, BaseProjectileType, damage, knockback, player.whoAmI).whoAmI;
            Projectile proj = Main.projectile[projIndex];

            // 设定 AI 模式（这里使用基类的 ProjectileAIMode）
            proj.ai[0] = ProjectileAIMode;

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe(20)
            .AddIngredient(ItemID.Diamond, 1)
            .AddIngredient(ItemID.Topaz, 1)
            .AddTile(TileID.WorkBenches)
            .Register();
        }
    }
}