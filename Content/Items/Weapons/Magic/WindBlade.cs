using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Projectiles.Magic;

namespace WuDao.Content.Items.Weapons.Magic
{
    // 风刃
    public class WindBlade : ModItem
    {
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Magic;
            Item.width = 28;
            Item.height = 30;
            Item.damage = 17;
            Item.knockBack = 2.2f;
            Item.mana = 7;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<WindBladeProjectile>();
            Item.shootSpeed = 0f; // 速度在弹幕里控制
            Item.autoReuse = true;
            Item.rare = ModContent.RarityType<LightBlueRarity>();
            Item.value = Item.buyPrice(silver: 50);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int bladeCount = 2;            // 一次生成 2 道
            float radiusMin = 36f;         // 出生半径范围
            float radiusMax = 48f;
            Vector2 mouseWorld = Main.MouseWorld;

            for (int i = 0; i < bladeCount; i++)
            {
                // 固定 18 个角度之一
                int angleIndex = Main.rand.Next(18);
                float angle = MathHelper.TwoPi * angleIndex / 18f;

                // 半径在范围内随机
                float radius = MathHelper.Lerp(radiusMin, radiusMax, Main.rand.NextFloat());
                Vector2 spawnPos = player.Center + angle.ToRotationVector2() * radius;

                // 如果点在物块里就跳过
                if (Collision.SolidCollision(spawnPos, 1, 1))
                    continue;

                int proj = Projectile.NewProjectile(
                    source,
                    spawnPos,
                    Vector2.Zero,  // 初速为 0：先滞留
                    type,
                    damage,
                    knockback,
                    player.whoAmI,
                    ai0: 0f,
                    ai1: 0f
                );

                Projectile p = Main.projectile[proj];
                p.localAI[0] = mouseWorld.X;
                p.localAI[1] = mouseWorld.Y;

                p.ai[0] = -Main.rand.Next(0, 6); // 额外等待 0~5 帧
                p.alpha = 200;
                p.netUpdate = true;
            }

            return false; // 手动生成
        }
        // 手持时向内偏移
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-1, 3); // 向内偏移3像素
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Book, 1)
                .AddIngredient(ItemID.HarpyWings, 2)
                .AddIngredient(ItemID.Cloud, 10)
                .AddIngredient(ItemID.SunplateBlock, 10)
                .AddTile(TileID.Bookcases);
        }
    }
}
