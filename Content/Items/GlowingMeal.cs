
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using WuDao.Common.Buffs;

namespace WuDao.Content.Items
{
    /// <summary>
    /// 发光料理：给予 WellFed3（吃得饱到发光），并从定义的增益池中随机获得 5 种。
    /// </summary>
    public class GlowingMeal : ModItem
    {
        public override void SetStaticDefaults()
        {
            // ① 注册物品竖直动画：6tick 切一帧，共3帧（可按喜好调快/调慢）
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(18 / 3, 3));

            // ② 标记为“食物”，显示刀叉角标，并启用吃东西时的粒子
            ItemID.Sets.IsFood[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 22;
            Item.maxStack = 2;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.UseSound = SoundID.Item2;
            Item.consumable = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(0, 3, 0);
            Item.noMelee = true;
        }

        public override bool? UseItem(Player player)
        {
            // 强力食物增益（1.4.4：WellFed3 = Exquisitely Stuffed）
            player.AddBuff(BuffID.WellFed3, 60 * 60 * 2); // 2 分钟

            // 增益池（尽量选通用而稳定的 Buff）
            int[] pool = new int[] {
                BuffID.Regeneration,
                BuffID.Swiftness,
                BuffID.Ironskin,
                BuffID.Endurance,
                BuffID.MagicPower,
                BuffID.Archery,
                BuffID.Thorns,
                BuffID.Lifeforce,
                BuffID.Mining,
                BuffID.Shine,
                BuffID.NightOwl,
                ModContent.BuffType<SweetRegen>(),
                ModContent.BuffType<SweetAgile>(),
                ModContent.BuffType<SweetLucky>(),
            };

            // 随机抽取 5 个不同的
            Shuffle(pool, Main.rand);
            int count = Math.Min(5, pool.Length);
            for (int i = 0; i < count; i++)
            {
                player.AddBuff(pool[i], 60 * 60 * 2); // 2 分钟
            }
            return true;
        }

        // Fisher-Yates shuffle
        private void Shuffle(int[] array, UnifiedRandom rand)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                int temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
    }
}
