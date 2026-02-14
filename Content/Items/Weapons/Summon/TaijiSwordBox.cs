using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Summon;
using WuDao.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Weapons.Summon
{
    /// <summary>
    /// 太极剑匣
    /// </summary>
    public class TaijiSwordBox : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.EnchantedSword}";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            ItemID.Sets.StaffMinionSlotsRequired[Item.type] = 1f;
        }

        public override void SetDefaults()
        {
            // 可以克隆雀杖手感
            Item.CloneDefaults(ItemID.Smolstar);

            Item.damage = 32;
            Item.knockBack = 2.5f;
            Item.mana = 10;

            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item44;

            Item.shoot = ModContent.ProjectileType<TaijiSwordMinion>();
            Item.buffType = ModContent.BuffType<TaijiSwordBoxBuff>();

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 2, 0, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
    Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            int projType = ModContent.ProjectileType<TaijiSwordMinion>();

            // 当前已有的太极剑数量（每次只+1，直到2）
            int current = player.ownedProjectileCounts[projType];

            if (current > player.maxMinions || current == 2)
                return false;

            Projectile.NewProjectile(
                source,
                Main.MouseWorld,
                Vector2.Zero,
                projType,
                damage,
                knockback,
                player.whoAmI,
                ai0: current % player.maxMinions,
                ai1: 0f
            );

            return false; // 手动生成
        }

        public override void AddRecipes()
        {
            // 配方 A：太极链枷 断钢剑
            Recipe.Create(Type)
                .AddIngredient(ItemID.Excalibur)
                .AddIngredient(ItemID.DaoofPow, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}