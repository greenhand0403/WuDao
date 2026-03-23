using WuDao.Content.Projectiles.Ranged;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Items.Weapons.Ranged
{
    // 神圣裁决 霰弹枪
    public class BrightVerdict : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.OnyxBlaster); // 继承霰弹逻辑基础参数
            Item.damage = 25;
            Item.shoot = ModContent.ProjectileType<BrightVerdictProjectile>(); // 特殊“神圣弹丸”
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item36;         // 霰弹风格，神圣一点也可换成SoundID.Item40等
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
    Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return false;

            int pelletCount = 4 + Main.rand.Next(2);

            for (int i = 0; i < pelletCount; i++)
            {
                Vector2 perturbed = velocity.RotatedByRandom(MathHelper.ToRadians(6));
                float speedScale = 0.9f + Main.rand.NextFloat(0.2f);
                Vector2 v = perturbed * speedScale;

                Projectile.NewProjectile(
                    source, position, v,
                    type,
                    (int)(damage * 0.65f),
                    knockback * 0.8f,
                    player.whoAmI
                );
            }

            Vector2 holyDir = velocity.SafeNormalize(Vector2.UnitX) * velocity.Length() * 1.05f;
            Projectile.NewProjectile(
                source, position, holyDir,
                ModContent.ProjectileType<BrightVerdictProjectile>(),
                (int)(damage * 1.15f),
                knockback,
                player.whoAmI
            );

            return false;
        }
        // 手持时向内偏移
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4, -2); // 向内偏移4像素
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Shotgun)
                .AddIngredient(ItemID.LightShard, 2)
                .AddIngredient(ItemID.SoulofLight, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
