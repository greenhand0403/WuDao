using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Magic;

namespace WuDao.Content.Items.Weapons.Magic
{
    public class WindBlade : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Wind Blade"); // 如需英文名可打开本行
            // Tooltip.SetDefault("Conjures wind blades that form around you, then surge towards the cursor.");
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;

            Item.DamageType = DamageClass.Magic;
            Item.damage = 17;                // 比水矢(19)略低
            Item.knockBack = 2.2f;
            Item.crit = 4;

            Item.mana = 7;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.UseSound = SoundID.Item8;   // 轻盈的魔法声
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<WindBladeProjectile>();
            Item.shootSpeed = 0f;            // 我们在 Shoot 里自定义速度
            Item.rare = ItemRarityID.Green;  // 同时期稀有度
            Item.value = Item.buyPrice(silver: 50);
        }

        // 在天磨(Sky Mill)配方合成：鸟羽x2、云x10、日盘块x10、火花魔棒x1
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Feather, 2)
                .AddIngredient(ItemID.Cloud, 10)
                .AddIngredient(ItemID.SunplateBlock, 10)
                .AddIngredient(ItemID.WandofSparking, 1)
                .AddTile(TileID.SkyMill)
                .Register();
        }

        // 关键：在玩家周围生成若干风刃，再朝鼠标飞行
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity,
            int type, int damage, float knockback)
        {
            // 环绕参数
            int bladeCount = 4;           // 生成 4 把风刃（可调：3~6）
            float radius = 48f;           // 围绕半径
            float speed = 12f;            // 朝鼠标的飞行速度
            var mouseWorld = Main.MouseWorld;

            for (int i = 0; i < bladeCount; i++)
            {
                float angle = MathHelper.TwoPi * i / bladeCount;
                Vector2 spawnPos = player.Center + radius * angle.ToRotationVector2();

                // 计算朝鼠标方向的速度
                Vector2 dir = (mouseWorld - spawnPos).SafeNormalize(Vector2.UnitX) * speed;

                int proj = Projectile.NewProjectile(
                    source,
                    spawnPos,
                    dir,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );

                // 小优化：开局轻微透明，手感更顺滑
                Main.projectile[proj].alpha = 40;
            }

            // 我们自己生成了弹幕，返回 false 阻止 tML 再生成一次
            return false;
        }
    }
}
