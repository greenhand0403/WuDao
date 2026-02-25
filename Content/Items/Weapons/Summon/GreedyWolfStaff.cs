using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Items.Weapons.Summon
{
    public class GreedyWolfStaff : ModItem
    {
        // 这里先复用一个原版召唤杖贴图，你也可以换成自己的
        public override string Texture => "Terraria/Images/Item_" + ItemID.SlimeStaff;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;

            // 占 1 个召唤栏
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.SlimeStaff);

            Item.damage = 40;
            Item.knockBack = 2.5f;
            Item.mana = 10;

            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<GreedyWolfMinion>();
            Item.buffType = ModContent.BuffType<GreedyWolfBuff>();

            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item44;
            Item.value = Item.buyPrice(0, 2, 0, 0);
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 经典召唤杖：在鼠标位置召唤
            player.AddBuff(Item.buffType, 2);

            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            projectile.originalDamage = Item.damage;

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LunarTabletFragment, 2)
                .AddIngredient(ItemID.LihzahrdPowerCell)
                .AddIngredient(ItemID.WolfBanner)
                .AddTile(TileID.LihzahrdFurnace)
                .Register();
        }
    }
}
