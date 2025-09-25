using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Items.Weapons.Summon
{
    // 附魔皇家权杖（召唤杖）
    public class EnchantedRoyalScepter : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.RoyalScepter}";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("附魔皇家权杖");
            // Tooltip.SetDefault("召唤一把悬浮在头顶的皇家权杖，自动发射穿墙暗影束\n" +
            //    "同时只能存在一把\n" +
            //    "空闲召唤栏位越多，造成的伤害越高");
            ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.mana = 10;
            Item.UseSound = SoundID.Item44; // 召唤杖音效
            Item.noMelee = true;

            Item.DamageType = DamageClass.Summon;
            Item.damage = 26;
            Item.knockBack = 1f;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(0, 5);

            // 召唤类：给玩家一个 buff，并生成“悬浮权杖”随从投射物
            Item.buffType = ModContent.BuffType<EnchantedRoyalScepterBuff>();
            Item.shoot = ModContent.ProjectileType<RoyalScepterMinion>();
            Item.shootSpeed = 0f; // 我们自己在投射物里控制位置与AI
        }

        public override bool CanUseItem(Player player)
        {
            // 只允许存在一把：如果已经有，允许再次使用刷新/迁移位置
            return true;
        }

        public override bool? UseItem(Player player)
        {
            // 只给 buff；不再手动生成投射物
            player.AddBuff(Item.buffType, 2);
            return true;
        }

        // 阻止自动发射 Item.shoot，避免和 Buff.Update 的生成重复
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.RoyalScepter)
                .AddIngredient(ItemID.LifeFruit, 5)
                .AddIngredient(ItemID.HallowedKey)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
