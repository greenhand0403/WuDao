using WuDao.Content.Projectiles.Ranged;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Items.Weapons.Ranged
{
    public class BrightVerdict : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.OnyxBlaster); // 继承霰弹逻辑基础参数
            Item.damage = 25;                        // 你原来的数值
            Item.shoot = ModContent.ProjectileType<BrightVerdictProjectile>(); // 特殊“神圣弹丸”
            Item.rare = ItemRarityID.LightRed;      // 可按喜好
            Item.UseSound = SoundID.Item36;         // 霰弹风格，神圣一点也可换成SoundID.Item40等
        }

        // 关键：自定义发射逻辑，复刻“霰弹 + 特殊弹”
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 霰弹数量（玛瑙爆破枪是 4~5）
            int pelletCount = 4 + Main.rand.Next(2);

            // 霰弹使用【玩家使用的子弹类型】(type 参数就是当前弹药转换后的 proj)
            for (int i = 0; i < pelletCount; i++)
            {
                // 霰弹散射
                Vector2 perturbed = velocity.RotatedByRandom(MathHelper.ToRadians(6)); // 扩散角
                float speedScale = 0.9f + Main.rand.NextFloat(0.2f);                   // 速度轻微抖动
                var v = perturbed * speedScale;

                // 发射霰弹（用弹药proj：type）
                Projectile.NewProjectile(
                    source, position, v,
                    type,                                           // 子弹
                    (int)(damage * 0.65f),                          // 霰弹单发低点伤害
                    knockback * 0.8f, player.whoAmI
                );
            }

            // 额外发一枚“神圣弹丸”（你自定义的特效弹，类似玛瑙的黑弹）
            // 稍微加速、穿透感强一点
            Vector2 holyDir = velocity.SafeNormalize(Vector2.UnitX) * velocity.Length() * 1.05f;
            Projectile.NewProjectile(
                source, position, holyDir,
                ModContent.ProjectileType<BrightVerdictProjectile>(), // 神圣弹
                (int)(damage * 1.15f),                                // 比霰弹更猛些
                knockback, player.whoAmI
            );
            
            // 白色“枪口火舌”（可选）
            for (int i = 0; i < 6; i++)
            {
                Vector2 dir = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(10));
                int d = Dust.NewDust(position, 0, 0, DustID.GemDiamond, 0f, 0f, 0, default, 1.2f);
                var dd = Main.dust[d];
                dd.noGravity = true;
                dd.velocity = dir * (6f + Main.rand.NextFloat(2f));
            }
            // 返回 false 以阻止默认再发一次（我们已手动生成所有弹）
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Shotgun)
                .AddIngredient(ItemID.LightShard, 2) // 保留你的设定
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
