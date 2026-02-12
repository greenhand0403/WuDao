using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Buffs;
using WuDao.Content.Projectiles.Summon;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Items.Weapons.Summon
{
    public class FlyingSnakeStaff : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.ImpStaff}";

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;

            Item.mana = 10;
            Item.damage = 42;                  // 你可再微调
            Item.knockBack = 2f;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;

            Item.DamageType = DamageClass.Summon;    // 召唤伤害
            Item.UseSound = SoundID.Item44;          // 与大多召唤杖一致
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.buyPrice(0, 5, 0, 0);

            Item.buffType = ModContent.BuffType<FlyingSnakeBuff>();
            Item.shoot = ModContent.ProjectileType<FlyingSnakeMinion>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LunarTabletFragment, 2)
                .AddIngredient(ItemID.LihzahrdPowerCell)
                .AddIngredient(ItemID.FlyingSnakeBanner)
                .AddTile(TileID.LihzahrdFurnace)
                .Register();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 在鼠标附近或玩家身边生成一个 minion
            Vector2 spawnPos = Main.MouseWorld;
            if (Collision.SolidCollision(spawnPos, 1, 1))
            {
                spawnPos = player.Center;
            }

            Projectile.NewProjectile(
                source,
                spawnPos,
                Vector2.Zero,
                type,
                damage,
                knockback,
                player.whoAmI
            );

            return false; // 我们手动生成，返回 false 不再用默认发射
        }
    }
}