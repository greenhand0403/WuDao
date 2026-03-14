using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Items.Weapons.Summon
{
    // 奇异口哨：召唤怪物火车仆从
    public class MonsterTrainStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Type] = true;
            ItemID.Sets.StaffMinionSlotsRequired[Item.type] = 1f;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.UseSound = SoundID.Item44;

            Item.damage = 40;
            Item.knockBack = 2f;
            Item.mana = 10;
            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<MonsterTrainMinion>();
            Item.buffType = ModContent.BuffType<MonsterTrainBuff>();
            Item.shootSpeed = 0f; // 仆从自己移动
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.buyPrice(0, 2, 0, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            int current = player.ownedProjectileCounts[type];

            if (current > player.maxMinions || current == 8)
                return false;

            Projectile.NewProjectile(
                source,
                Main.MouseWorld,
                Vector2.Zero,
                type,
                damage,
                knockback,
                player.whoAmI,
                ai0: 0,
                ai1: 0,
                ai2: current % 8
            );

            return false;
        }
        public override void AddRecipes()
        {
            Recipe.Create(Type)
                .AddIngredient(ItemID.MinecartMech)
                .AddIngredient(ItemID.PygmyStaff, 1)
                .AddIngredient(ItemID.FragmentStardust, 8)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}