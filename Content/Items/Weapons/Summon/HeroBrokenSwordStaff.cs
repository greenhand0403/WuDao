using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Items.Weapons.Summon
{
    // 附魔英雄断剑
    public class HeroBrokenSwordStaff : ModItem
    {
        // 物品贴图直接复用断裂英雄剑
        public override string Texture => "Terraria/Images/Item_" + ItemID.BrokenHeroSword;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            // 占 1 个召唤栏（保持与刃杖一致）
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }

        public override void SetDefaults()
        {
            // 从原版刃杖克隆基础手感（施法后摇、稀有度、卖价等）
            Item.CloneDefaults(ItemID.Smolstar);

            Item.damage = 45;
            Item.mana = 10;
            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item44;
            Item.rare = ItemRarityID.Yellow;
            Item.buffType = ModContent.BuffType<HeroBrokenSwordBuff>();
            Item.shoot = ModContent.ProjectileType<HeroBrokenSwordMinion>();
        }

        // 让召唤物出生在鼠标位置（与多数原版召唤一致）
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Buff 持续 & 手动生成投射物并回填 originalDamage（示例中的标准做法）
            player.AddBuff(Item.buffType, 2);
            var proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            proj.originalDamage = Item.damage;   // 确保随重新铸造/加成正确缩放
            return false; // 已手动生成，阻止默认生成
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BrokenHeroSword)
                .AddIngredient(ItemID.Smolstar)
                .AddIngredient(ItemID.HallowedBar, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
