using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Weapons.Melee
{
    // 爆破钳
    class BlasterPliers : ModItem
    {
        public override void SetDefaults()
        {
            // Item.CloneDefaults(ItemID.CombatWrench);
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.damage = 30;
            Item.noMelee = false;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shoot = ProjectileID.None;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
            Item.width = 42;
            Item.height = 42;
            Item.rare = ModContent.RarityType<LightBlueRarity>();
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // 直接统计本次结算时玩家背包里的热狗数量（联机安全）
            int hotdogs = 0;
            for (int i = 0; i < player.inventory.Length; i++)
            {
                if (player.inventory[i].type == ItemID.Hotdog)
                    hotdogs += player.inventory[i].stack;
            }
            damage.Flat += hotdogs * 0.1f;
        }
        // 手持时向内偏移
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4, 2); // 向内偏移4像素
        }
        // 根据敌怪稀有度增加伤害
        // public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        // {
        //     int g = target.rarity;
        //     if (g > 0)
        //     {
        //         float multiplier = 1f + g * 0.5f;
        //         modifiers.FinalDamage *= multiplier;

        //         CombatText.NewText(
        //             target.Hitbox,
        //             new Color(180, 220, 255),
        //             $"敌怪稀有度 {g}"
        //         );
        //     }
        // }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.OldShoe, 4)
                .AddTile(TileID.TinkerersWorkbench)
                .AddCondition(Condition.InJungle)
                .Register();
        }
    }
}