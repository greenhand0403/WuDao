// New file: Systems/CuisinePlayer.cs
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.IO;
using Terraria.ID;

namespace WuDao.Content.Players
{
    public class CuisinePlayer : ModPlayer
    {
        // 背包是否携带“菜谱/食谱”类物品（你已有就保留）
        public bool HasCookbook;
        public bool HasFoodLogItem;

        /// <summary>
        /// 厨艺数：已“合成过”的食物（用于‘菜谱轮换’池）
        /// </summary>
        public readonly HashSet<int> CraftedFoodTypes = new();

        /// <summary>
        /// 美味值：已“吃过”的食物（全量统计，含不可合成 & 模组食物）——第③点用到
        /// </summary>
        public readonly HashSet<int> FoodsEatenAll = new();

        // —— 新增：每天的“起始索引”，用于两道菜谱定位 —— 
        public int TodayStartIndex;
        public int TodayStartDay; // 记录上次刷新的 DayCounter
        // —— 新增：食谱随机 6 个未品尝条目 —— 
        public readonly List<int> SuggestedFoods6 = new(6);
        private bool _foodLogLastTick = false;
        /// <summary>
        /// 厨艺数：用于“首次制作”判定（独立于奖励资格池）
        /// </summary>
        public readonly HashSet<int> CraftedEverFoods = new();  // 曾经制作过的食物（无论是否携带菜谱）

        /// <summary>
        /// 美味值：用于“首次品尝”判定（独立于奖励资格池）
        /// </summary>
        public readonly HashSet<int> EatenEverFoods = new();  // 曾经吃过的食物（无论是否携带收藏）

        // —— 新增：总数值 —— 
        /// <summary>
        /// 厨艺数：首次制作时 += 食物.buffTime
        /// </summary>
        public int CookingSkill;
        /// <summary>
        /// 美味值：首次品尝时 += 食物.buffTime
        /// </summary>
        public int Deliciousness;
        public override void ResetEffects()
        {
            HasCookbook = false;
            HasFoodLogItem = false;
        }
        public override void PostUpdate()
        {
            // 监听“收藏状态”变化：false->true 时刷新；true->false 时清空
            if (HasFoodLogItem && !_foodLogLastTick)
            {
                RefreshSuggestedFoods6();
            }
            else if (!HasFoodLogItem && _foodLogLastTick)
            {
                SuggestedFoods6.Clear();
            }
            _foodLogLastTick = HasFoodLogItem;
        }

        public void RefreshSuggestedFoods6()
        {
            SuggestedFoods6.Clear();
            // 收集所有“未品尝”的食物
            List<int> pool = new();
            for (int type = 1; type < ItemLoader.ItemCount; type++)
                if (ItemID.Sets.IsFood[type] && !FoodsEatenAll.Contains(type))
                    pool.Add(type);

            // 乱序抽取 6 个
            ShuffleList(pool, Main.rand);
            int take = System.Math.Min(6, pool.Count);
            for (int i = 0; i < take; i++) SuggestedFoods6.Add(pool[i]);
        }

        // Fisher-Yates shuffle
        private void ShuffleList<T>(List<T> list, Terraria.Utilities.UnifiedRandom rand)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["CraftedFoodTypes"] = new List<int>(CraftedFoodTypes);
            tag["FoodsEatenAll"] = new List<int>(FoodsEatenAll);
            tag["TodayStartIndex"] = TodayStartIndex;
            tag["TodayStartDay"] = TodayStartDay;
            tag["CraftedEverFoods"] = new List<int>(CraftedEverFoods);
            tag["CookingSkill"] = CookingSkill;
            tag["Deliciousness"] = Deliciousness;
        }
        public override void LoadData(TagCompound tag)
        {
            CraftedFoodTypes.Clear();
            FoodsEatenAll.Clear();
            CraftedEverFoods.Clear();
            if (tag.ContainsKey("CraftedFoodTypes")) CraftedFoodTypes.UnionWith(tag.Get<List<int>>("CraftedFoodTypes"));
            if (tag.ContainsKey("FoodsEatenAll")) FoodsEatenAll.UnionWith(tag.Get<List<int>>("FoodsEatenAll"));
            if (tag.ContainsKey("CraftedEverFoods")) CraftedEverFoods.UnionWith(tag.Get<List<int>>("CraftedEverFoods"));
            if (tag.ContainsKey("CookingSkill")) CookingSkill = tag.GetInt("CookingSkill");
            if (tag.ContainsKey("Deliciousness")) Deliciousness = tag.GetInt("Deliciousness");
            TodayStartIndex = tag.GetInt("TodayStartIndex");
            TodayStartDay = tag.GetInt("TodayStartDay");
        }
    }
}
