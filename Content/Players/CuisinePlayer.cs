using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace WuDao.Content.Players
{
    // 厨艺和美食系统
    public class CuisinePlayer : ModPlayer
    {
        // 背包是否携带“菜谱/食谱”类物品
        public bool HasCookbook;
        public bool HasFoodLogItem;

        /// <summary>
        /// 厨艺数：已“合成过”的食物（用于‘菜谱轮换’池）
        /// </summary>
        public readonly HashSet<int> CraftedFoodTypes = new();

        /// <summary>
        /// 美味值：已“吃过”的食物（全量统计，含不可合成 & 模组食物）
        /// </summary>
        public readonly HashSet<int> FoodsEatenAll = new();

        // 每天两道菜谱定位
        public int TodayStartIndex;
        public int TodayStartDay;

        // 食谱建议 6 个未品尝条目（纯本地 UI）
        public readonly List<int> SuggestedFoods6 = new(6);
        private bool _foodLogLastTick = false;

        /// <summary>
        /// 厨艺数：用于“首次制作”判定（独立于奖励资格池）
        /// </summary>
        public readonly HashSet<int> CraftedEverFoods = new();

        /// <summary>
        /// 美味值：用于“首次品尝”判定（独立于奖励资格池）
        /// </summary>
        public readonly HashSet<int> EatenEverFoods = new();

        /// <summary>
        /// 厨艺值
        /// </summary>
        public int CookingSkill;

        /// <summary>
        /// 美味值
        /// </summary>
        public int Deliciousness;

        private bool _cuisineSyncPending;

        public override void ResetEffects()
        {
            HasCookbook = false;
            HasFoodLogItem = false;
        }

        public override void OnEnterWorld()
        {
            if (Player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
            {
                _cuisineSyncPending = true;
                SyncPlayer(-1, Main.myPlayer, false);
                _cuisineSyncPending = false;
            }
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
            {
                if (ItemID.Sets.IsFood[type] && !FoodsEatenAll.Contains(type))
                    pool.Add(type);
            }

            ShuffleList(pool, Main.rand);

            int take = System.Math.Min(6, pool.Count);
            for (int i = 0; i < take; i++)
                SuggestedFoods6.Add(pool[i]);
        }

        private void ShuffleList<T>(List<T> list, Terraria.Utilities.UnifiedRandom rand)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public void MarkCuisineDirty()
        {
            _cuisineSyncPending = true;
        }

        public override void CopyClientState(ModPlayer targetCopy)
        {
            CuisinePlayer clone = (CuisinePlayer)targetCopy;

            clone.CookingSkill = CookingSkill;
            clone.Deliciousness = Deliciousness;

            clone.CraftedEverFoods.Clear();
            clone.CraftedEverFoods.UnionWith(CraftedEverFoods);

            clone.EatenEverFoods.Clear();
            clone.EatenEverFoods.UnionWith(EatenEverFoods);
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            if (!_cuisineSyncPending)
                return;

            SyncPlayer(-1, Main.myPlayer, false);
            _cuisineSyncPending = false;
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.SyncCuisineState);
            packet.Write((byte)Player.whoAmI);

            packet.Write(CookingSkill);
            packet.Write(Deliciousness);

            packet.Write(CraftedEverFoods.Count);
            foreach (int t in CraftedEverFoods)
                packet.Write(t);

            packet.Write(EatenEverFoods.Count);
            foreach (int t in EatenEverFoods)
                packet.Write(t);

            packet.Send(toWho, fromWho);
        }

        public override void SaveData(TagCompound tag)
        {
            tag["CraftedFoodTypes"] = new List<int>(CraftedFoodTypes);
            tag["FoodsEatenAll"] = new List<int>(FoodsEatenAll);
            tag["CraftedEverFoods"] = new List<int>(CraftedEverFoods);
            tag["EatenEverFoods"] = new List<int>(EatenEverFoods);
            tag["CookingSkill"] = CookingSkill;
            tag["Deliciousness"] = Deliciousness;
            tag["TodayStartIndex"] = TodayStartIndex;
            tag["TodayStartDay"] = TodayStartDay;
        }

        public override void LoadData(TagCompound tag)
        {
            CraftedFoodTypes.Clear();
            FoodsEatenAll.Clear();
            CraftedEverFoods.Clear();
            EatenEverFoods.Clear();

            if (tag.ContainsKey("CraftedFoodTypes"))
                CraftedFoodTypes.UnionWith(tag.Get<List<int>>("CraftedFoodTypes"));

            if (tag.ContainsKey("FoodsEatenAll"))
                FoodsEatenAll.UnionWith(tag.Get<List<int>>("FoodsEatenAll"));

            if (tag.ContainsKey("CraftedEverFoods"))
                CraftedEverFoods.UnionWith(tag.Get<List<int>>("CraftedEverFoods"));

            if (tag.ContainsKey("EatenEverFoods"))
                EatenEverFoods.UnionWith(tag.Get<List<int>>("EatenEverFoods"));

            if (tag.ContainsKey("CookingSkill"))
                CookingSkill = tag.GetInt("CookingSkill");

            if (tag.ContainsKey("Deliciousness"))
                Deliciousness = tag.GetInt("Deliciousness");

            TodayStartIndex = tag.GetInt("TodayStartIndex");
            TodayStartDay = tag.GetInt("TodayStartDay");
        }
    }
}