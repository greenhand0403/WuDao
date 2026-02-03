
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Items
{
    /// <summary>
    /// 喜糖：获得小量的再生/敏捷/幸运（自定义 Buff，避免跨版本 ID 差异）
    /// </summary>
    public class Candy : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 5;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useTime = 17;
            Item.useAnimation = 17;
            Item.UseSound = SoundID.Item2;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0, 0, 50);
            Item.noMelee = true;
            Item.buffType = 0; // 不自动附加，在 OnConsumeItem 手动处理
        }

        public override bool? UseItem(Player player)
        {
            // 食物层：WellFed(小) 或 WellFed2(中)
            int[] foods = new int[] { BuffID.WellFed, BuffID.WellFed2 };
            int pickFood = Main.rand.Next(foods.Length);
            player.AddBuff(foods[pickFood], 60 * 60); // 1 分钟

            // 自定义池：再生/敏捷/幸运（自定义 Buff，避免跨版本 ID 差异）
            int[] pool = new int[] {
                ModContent.BuffType<SweetRegen>(),
                ModContent.BuffType<SweetAgile>(),
                ModContent.BuffType<SweetLucky>(),
            };
            int pick = Main.rand.Next(pool.Length);
            player.AddBuff(pool[pick], 60 * 60); // 1 分钟

            return true;
        }
    }
}
