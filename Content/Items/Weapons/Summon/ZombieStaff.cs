using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using WuDao.Content.Projectiles.Summon;
using WuDao.Content.Buffs;

namespace WuDao.Content.Items.Weapons.Summon
{
    public class ZombieStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            // 占 1 个召唤栏（保持与刃杖一致）
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }
        public override void SetDefaults()
        {
            Item.damage = 22;
            Item.DamageType = DamageClass.Summon;

            Item.mana = 10;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 36;
            Item.useAnimation = 36;

            Item.useStyle = ItemUseStyleID.Swing;

            Item.knockBack = 2;

            Item.value = Item.buyPrice(0, 0, 50);
            Item.rare = ItemRarityID.Blue;

            Item.UseSound = SoundID.Item44;

            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<ZombieMinion>();
            Item.buffType = ModContent.BuffType<ZombieMinionBuff>();

            Item.shootSpeed = 10f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            int projType = ModContent.ProjectileType<ZombieMinion>();

            // 当前已有的僵尸数量（每次只+1，直到2）
            int current = player.ownedProjectileCounts[projType];

            if (current > player.maxMinions || current == 2)
                return false;
                
            if (player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(
                    source,
                    position,
                    velocity,
                    type,
                    damage,
                    knockback,
                    player.whoAmI,
                    ai0: current % player.maxMinions
                );
            }

            return false;
        }
    }
}