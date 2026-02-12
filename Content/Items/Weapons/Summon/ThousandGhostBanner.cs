using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Buffs;
using WuDao.Content.Projectiles.Summon;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Items.Weapons.Summon
{
    public class ThousandGhostBanner : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.GhostBanner}";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;

            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f; // The default value is 1, but other values are supported. See the docs for more guidance. 
        }
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;

            Item.mana = 10;
            Item.damage = 40;
            Item.knockBack = 2f;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;

            Item.DamageType = DamageClass.Summon;    // 召唤伤害
            Item.UseSound = SoundID.Item44;          // 与大多召唤杖一致
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.buyPrice(0, 5, 0, 0);

            Item.buffType = ModContent.BuffType<ThousandGhostBannerBuff>();
            Item.shoot = ModContent.ProjectileType<ThousandGhostMinion>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LunarTabletFragment, 2)
                .AddIngredient(ItemID.LihzahrdPowerCell)
                .AddIngredient(ItemID.GhostBanner)
                .AddTile(TileID.LihzahrdFurnace)
                .Register();
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position
            position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
            player.AddBuff(Item.buffType, 2);

            // Minions have to be spawned manually, then have originalDamage assigned to the damage of the summon item
            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            projectile.originalDamage = Item.damage;

            // Since we spawned the projectile manually already, we do not need the game to spawn it for ourselves anymore, so return false
            return false;
        }
    }
}
