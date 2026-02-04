using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Projectiles.Ranged;
using WuDao.Content.Items.Ammo;

namespace WuDao.Content.Items.Weapons.Ranged
{
    // 丘比特弓
    public class CupidBow : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 58;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 22;
            Item.useTime = 22;
            Item.useAmmo = AmmoID.Arrow;
            Item.autoReuse = true;

            Item.noMelee = true;
            Item.damage = 21;
            Item.knockBack = 2f;
            Item.DamageType = DamageClass.Ranged;
            Item.shootSpeed = 9f;
            Item.shoot = ProjectileID.WoodenArrowFriendly; // 实际在 Shoot 里替换
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 2);
            Item.UseSound = SoundID.Item5;
        }

        private const float DoubleShotChance = 0.25f; // 25% 双发

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            // 双发只消耗1枚：第二支是我们手动再生成的，不触发这一钩子
            return true;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var mp = player.GetModPlayer<HeartStuffPlayer>();

            // 1) 木箭 -> 心箭
            if (type == ProjectileID.WoodenArrowFriendly)
                type = ModContent.ProjectileType<HeartArrowProj>();

            // 2) 丘比特弓的生命消耗（心灵宝石则为0）
            if (type == ModContent.ProjectileType<HeartArrowProj>())
            {
                int lifeCost = mp.GetHeartArrowLifeCost(usingCupidBow: true);
                if (lifeCost > 0 && player.statLife > lifeCost)
                {
                    player.statLife -= lifeCost;
                    player.HealEffect(-lifeCost, broadcast: true);
                }
            }

            // 3) —— 关键：出膛点前推，避免刚生成就与玩家/方块/近身敌人重叠 —— //
            // 计算发射方向（单位向量）
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);
            // 推进 40 像素（常用值；可按手感改 24~50）
            Vector2 muzzleOffset = dir * 40f;
            // 只有在这段路径“可命中”（无遮挡）时才前推，避免把弹丸塞进墙里
            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
                position += muzzleOffset;

            // 4) 发射第1支
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // 5) 双发：同样需要对第2支应用“出膛前推”
            if (Main.rand.NextFloat() < DoubleShotChance)
            {
                Vector2 v2 = velocity.RotatedByRandom(MathHelper.ToRadians(4));
                Vector2 pos2 = position; // 已经前推过的位置为基准
                // 再做一次极短的微前推，防止双发互相重叠立即碰撞（可选，2~6像素）
                Vector2 micro = v2.SafeNormalize(Vector2.UnitX) * 12f;
                pos2 += Vector2.UnitY * 12f;
                if (Collision.CanHit(pos2, 0, 0, pos2 + micro, 0, 0))
                    pos2 += micro;

                Projectile.NewProjectile(source, pos2, v2, type, (int)(damage * 0.9f), knockback, player.whoAmI);
            }

            return false; // 我们手动生成了弹幕
        }
        public override void AddRecipes()
        {
            // 丘比特弓：金弓+心箭若干+红心若干
            Recipe.Create(ModContent.ItemType<CupidBow>())
                .AddIngredient(ItemID.GoldBow)
                .AddIngredient(ModContent.ItemType<HeartArrow>(), 50)
                .AddIngredient(ItemID.LifeCrystal, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}