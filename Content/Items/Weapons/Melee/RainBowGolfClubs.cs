using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Items.Weapons.Melee
{
    public class RainBowGolfClubs : ModItem
    {
        private int ballIndex = 0;
        public override void SetDefaults()
        {
            // 近战武器参数（你可按需平衡）
            Item.damage = 22;
            Item.DamageType = DamageClass.MeleeNoSpeed; // 近战但不吃攻速作伤害加成
            Item.knockBack = 6f;

            Item.useStyle = ItemUseStyleID.GolfPlay; // 挥动
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.autoReuse = true;

            Item.width = 40;
            Item.height = 40;

            Item.noMelee = true;       // 不用近战挥击盒子造成伤害，伤害由射弹承担
            Item.noUseGraphic = false; // 仍显示武器贴图

            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Green;

            Item.UseSound = SoundID.Item1;

            Item.shootSpeed = 12f; // 基础朝向速度；实际会在 Shoot() 里做随机化
            Item.shoot = ProjectileID.GolfBallDyedBlack;
        }

        public override bool CanUseItem(Player player)
        {
            // 这里保持默认可用。若你想防止贴墙发射可加判定
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 颜色循环：按你之前的 ModPlayer 索引来
            int vanillaType = ProjectileID.GolfBallDyedBlack + ballIndex;
            ballIndex = (++ballIndex) % 14;
            // 随机偏角/速度（保持你原始手感）
            float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-12, 12));
            float speed = Main.rand.NextFloat(9f, 15f);
            Vector2 dir = velocity.SafeNormalize(player.direction == 1 ? Vector2.UnitX : -Vector2.UnitX);
            Vector2 finalVel = dir.RotatedBy(angleOffset) * speed;
            // 固定10点伤害
            int p = Projectile.NewProjectile(source, position, finalVel, vanillaType, 10 + ballIndex, knockback, player.whoAmI);

            // 暂时不在这里改近战/寿命/命中次数 —— 交给 GlobalProjectile 统一做（见下一节）
            return false;
        }

        public override void AddRecipes()
        {
            var rcp = CreateRecipe();
            for (int i = 0; i < 14; i++)
            {
                rcp.AddIngredient(ItemID.GolfBallDyedBlack + i, 1);
            }
            rcp.AddIngredient(ItemID.GolfClubIron, 1);
            rcp.AddTile(TileID.Anvils);
        }
    }
}
