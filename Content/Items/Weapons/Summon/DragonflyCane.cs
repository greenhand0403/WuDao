using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Summon;
using WuDao.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace WuDao.Content.Items.Weapons.Summon
{
    /// <summary>
    /// 裁决手杖，召唤蜻蜓
    /// </summary>
    public class DragonflyCane : ModItem
    {

        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            ItemID.Sets.StaffMinionSlotsRequired[Item.type] = 1f;
        }

        public override void SetDefaults()
        {
            // 可以克隆雀杖手感
            Item.CloneDefaults(ItemID.BabyBirdStaff);

            Item.damage = 11;                 // ≈ 吸血蛙法杖
            Item.knockBack = 2.5f;
            Item.mana = 10;

            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item44;

            Item.shoot = ModContent.ProjectileType<DragonflyMinion>();
            Item.buffType = ModContent.BuffType<DragonflyCaneBuff>();

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 2, 0, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
    Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            int projType = ModContent.ProjectileType<DragonflyMinion>();

            // 当前已有的蜻蜓数量（每次只+1，直到7）
            int current = player.ownedProjectileCounts[projType];

            if (current > player.maxMinions || current == 7)
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
            // 配方 A：任意7只蜻蜓 + 2魔力星 + 雀杖
            Recipe.Create(Type)
                .AddRecipeGroup(RecipeGroupID.Dragonflies, 7)
                .AddIngredient(ItemID.FallenStar, 2)
                .AddIngredient(ItemID.BabyBirdStaff, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }

    }
}