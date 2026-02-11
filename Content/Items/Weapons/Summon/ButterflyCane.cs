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
    // TODO: 从三个buff开始往后，补充贴图
    /// <summary>
    /// 蝴蝶召唤杖
    /// </summary>
    public class ButterflyCane : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.GoldButterfly}";

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

            Item.shoot = ModContent.ProjectileType<ButterflyMinion>();
            Item.buffType = ModContent.BuffType<ButterflyCaneBuff>();

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 2, 0, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
    Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            int projType = ModContent.ProjectileType<ButterflyMinion>();

            // 当前已有的蝴蝶数量（每次只+1，直到8）
            int current = player.ownedProjectileCounts[projType];

            // Main.NewText($"蝴蝶召唤杖：已有{current}只");
            if (current > player.maxMinions || current == 8)
                return false;

            Projectile.NewProjectile(
                source,
                Main.MouseWorld,
                Vector2.Zero,
                projType,
                damage,
                knockback,
                player.whoAmI,
                ai0: current % player.maxMinions,  // 这只蝴蝶的品种
                ai1: 0f
            );

            return false; // 手动生成
        }

        public override void AddRecipes()
        {
            // 配方 A：任意8只蝴蝶 + 2魔力星 + 雀杖
            Recipe.Create(Type)
                .AddRecipeGroup(RecipeGroupID.Butterflies, 8)
                .AddIngredient(ItemID.FallenStar, 2)
                .AddIngredient(ItemID.BabyBirdStaff, 1)
                .AddTile(TileID.Anvils)
                .Register();

            // 配方 B：任意8只蝴蝶 + 2魔力星 + 火花魔棒
            Recipe.Create(Type)
                .AddRecipeGroup(RecipeGroupID.Butterflies, 8)
                .AddIngredient(ItemID.FallenStar, 2)
                .AddIngredient(ItemID.WandofSparking, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }

    }
}