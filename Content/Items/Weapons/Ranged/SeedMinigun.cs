using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WuDao.Content.Players;
using Terraria.DataStructures;

namespace WuDao.Content.Items.Weapons.Ranged
{
    public class SeedMinigun : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.VenusMagnum;
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("种子机关枪");
        //     Tooltip.SetDefault(
        //         "快速发射5~8枚随机种子\n" +
        //         "随后冷却1秒\n" +
        //         "攻速只影响射弹速度"
        //     );
        // }

        public override void SetDefaults()
        {
            Item.damage = 46;
            Item.DamageType = DamageClass.Ranged;

            Item.width = 56;
            Item.height = 24;

            // 固定发射间隔 7 tick
            Item.useTime = 7;
            Item.useAnimation = 7;
            Item.reuseDelay = 0;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.knockBack = 2f;
            Item.value = Item.sellPrice(0, 8);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item34;

            // 使用火枪子弹
            Item.useAmmo = AmmoID.Bullet;

            // 占位，真正发射内容在 Shoot/ModifyShootStats 里改
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 12f;
        }

        // 固定射速，不受攻速影响
        public override float UseSpeedMultiplier(Player player) => 1f;

        public override void HoldItem(Player player)
        {
            // 持有时自动尝试补充一轮 5~8 发
            player.GetModPlayer<SeedMinigunPlayer>().TryRefillSeedMinigunAmmo();
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<SeedMinigunPlayer>();

            // 先尝试补充，避免“冷却结束后还得多点一下才恢复”
            modPlayer.TryRefillSeedMinigunAmmo();

            // 只有存在可发射子弹时才能开火
            return modPlayer.seedMinigunShots > 0;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-4f, 0f);

        public override void ModifyShootStats(
            Player player,
            ref Vector2 position,
            ref Vector2 velocity,
            ref int type,
            ref int damage,
            ref float knockback)
        {
            // 攻速只影响射弹飞行速度
            float attackSpeed = player.GetAttackSpeed(DamageClass.Ranged);
            velocity *= attackSpeed;

            // 将火枪子弹转换为随机种子射弹
            type = Main.rand.Next(3) switch
            {
                0 => ProjectileID.SeedlerNut,         // 坚果
                1 => ProjectileID.SeedPlantera,       // 世纪之花种子
                _ => ProjectileID.PoisonSeedPlantera  // 毒种子
            };
            // 增加坚果射弹的速度
            if (type == ProjectileID.SeedlerNut)
                velocity *= 1.3f;

            // 稍微给一点散射，更像机关枪
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(3.5f));
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
            var modPlayer = player.GetModPlayer<SeedMinigunPlayer>();

            if (modPlayer.seedMinigunShots <= 0)
                return false;

            // 这里的 damage 已经包含：
            // 武器基础伤害 + 当前消耗的火枪子弹伤害贡献
            int proj = Projectile.NewProjectile(
                source,
                position,
                velocity,
                type,
                damage,
                knockback,
                player.whoAmI
            );

            Main.projectile[proj].hostile = false;
            Main.projectile[proj].friendly = true;

            modPlayer.seedMinigunShots--;

            return false;
        }
    }
}