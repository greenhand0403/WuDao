using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Items.Weapons.Melee
{
    // 电弧绘制参考原版projectile里面的闪电珠弧AI和main里面的绘制电弧线段的方法
    public class StormwrathBlade : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;

            Item.damage = 72;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6f;
            Item.crit = 4;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useTurn = true;
            Item.autoReuse = true;

            Item.noUseGraphic = false;
            Item.noMelee = false;

            Item.UseSound = SoundID.Item1;

            // Item.shoot = ProjectileID.CultistBossLightningOrbArc;
            Item.shoot = ModContent.ProjectileType<StormwrathLightning>();
            Item.shootSpeed = 0f;

            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 10, 0, 0);
        }

        public override bool Shoot(
            Player player,
            EntitySource_ItemUse_WithAmmo source,
            Vector2 position,
            Vector2 velocity,
            int type,
            int damage,
            float knockback)
        {
            if (Main.netMode == NetmodeID.Server)
                return false;

            if (player.whoAmI != Main.myPlayer)
                return false;

            // 屏幕宽度的0.2~0.8的相对位置
            // Vector2 target = Main.screenPosition + new Vector2(Main.screenWidth * Main.rand.NextFloat(0.2f, 0.8f), 0);
            // 在玩家上方的屏幕位置，左右浮动80像素
            Vector2 target = player.Center + new Vector2(0, -Main.screenHeight / 2) + new Vector2(Main.rand.NextFloat(-80f, 80f), 0);

            // 落点目标位置是鼠标位置
            Vector2 spawnPos = Main.MouseWorld;
            Vector2 direction = Vector2.Normalize(spawnPos - target).RotatedByRandom(0.7853981852531433);
            int proj = Projectile.NewProjectile(
                source,
                target,// 生成位置
                direction * 7f, // 速度7f
                type,
                damage,
                knockback,
                player.whoAmI,
                direction.ToRotation(),
                Main.rand.Next(100)
            );
            Main.projectile[proj].friendly = true;
            Main.projectile[proj].hostile = false;
            Main.projectile[proj].netUpdate = true;
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.StarWrath);
            recipe.AddIngredient(ItemID.LunarBar, 8);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}