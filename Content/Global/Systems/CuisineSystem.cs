// New file: Systems/CuisineSystem.cs
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Terraria.Localization;
using Microsoft.Xna.Framework; // for Color
using WuDao.Content.Players;
using Terraria.Utilities;
using System.IO;
using WuDao.Content.Items;
using Terraria.GameContent.ItemDropRules;
using System.Text;
using System.Linq; // DropRateInfo / IItemDropRule / ReportDroprates

namespace WuDao.Content.Global.Systems
{
    public class CuisineSystem : ModSystem
    {
        // CuisineSystem.cs 里（类内静态区域）
        public static class FoodHintText
        {
            public const string TravelingMerchantInSnow = "旅商在雪地环境下售卖";
            public const string TravelingMerchant = "旅商有时会售卖";
            public const string Fishing = "钓鱼获得";
            public const string ShakeForestTrees = "摇晃森林的树木获得";
            public const string ShakeOceanPalms = "摇晃海洋的棕榈树获得";
            public const string ShakeSnowTrees = "摇晃雪地的树木获得";
            public const string ShakeJungleTrees = "摇晃丛林的树木获得";
            public const string ShakeHellTrees = "摇晃地狱的树木获得";
            public const string ShakeHolyTrees = "摇晃神圣地树木获得";
            public const string GiftBag = "礼物袋掉落";
            public const string Chest = "宝匣中获得";
            public const string BoughtFromNPC = "从商人处购买";
            // …按需要继续补充
        }

        private static readonly int[] VanillaFoodWhitelist = new int[] {
            ItemID.GoldenDelight,
            ItemID.PrismaticPunch,
            ItemID.FruitSalad,
            ItemID.SeafoodDinner,
            ItemID.GrubSoup,
            ItemID.TropicalSmoothie,
            ItemID.SmoothieofDarkness,
            ItemID.PinaColada,              // 注意：没有波浪符，枚举写 PinaColada
            ItemID.BloodyMoscato,
            ItemID.RoastedDuck,
            ItemID.LobsterTail,
            ItemID.FruitJuice,
            ItemID.BananaDaiquiri,
            ItemID.Escargot,
            ItemID.CookedShrimp,
            ItemID.Sashimi,
            ItemID.PumpkinPie,
            ItemID.MonsterLasagna,
            ItemID.BowlofSoup,
            ItemID.SauteedFrogLegs,
            ItemID.RoastedBird,
            ItemID.PeachSangria,
            ItemID.Lemonade,
            ItemID.GrilledSquirrel,
            ItemID.CookedMarshmallow,
            ItemID.BunnyStew,
            ItemID.AppleJuice,
            ItemID.FroggleBunwich,
            ItemID.CookedFish,
            ItemID.Teacup,
        };
        // 当日精选的“食物”itemType（0 表示无）
        public static int TodayFeaturedFoodType;

        // 最终可用的“菜谱池”（仅包含：被标为食物 + 有配方 的物品）
        public static readonly List<int> FoodPool = new();
        public static int DayCounter; // 世界范围的“天数”计数
                                      // 先放在类里（静态字典）
        public static readonly Dictionary<int, string> ManualFoodHints = new();
        public static int TotalFoodCount { get; private set; }
        public override void PostUpdateWorld()
        {
            // 每到清晨 4:30（dayTime && time==0）记为新的一天
            if (Main.dayTime && Main.time == 0)
                DayCounter++;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["DayCounter"] = DayCounter;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            DayCounter = tag.GetInt("DayCounter");
        }

        public override void OnWorldLoad()
        {
            RebuildFoodPool();
            TotalFoodCount = 0;
            for (int type = 1; type < ItemLoader.ItemCount; type++)
            {
                if (ItemID.Sets.IsFood[type])
                    TotalFoodCount++;
            }
            ManualFoodHints[ItemID.Marshmallow] = FoodHintText.TravelingMerchantInSnow;
            ManualFoodHints[ItemID.Elderberry] = "摇晃腐化环境树和棕榈树";
            ManualFoodHints[ItemID.BlackCurrant] = "摇晃腐化环境树和棕榈树";
            ManualFoodHints[ItemID.BloodOrange] = "摇晃猩红环境树和棕榈树";
            ManualFoodHints[ItemID.Rambutan] = "摇晃猩红环境树和棕榈树";
            ManualFoodHints[ItemID.ShuckedOyster] = "打开牡蛎(沙漠环境钓鱼)获得";
            RegisterHint(ItemID.CookedMarshmallow, "篝火处烤棉花糖棍(棉花糖+木材)");
            RegisterHint(ItemID.JojaCola, FoodHintText.Fishing);
            RegisterHint(ItemID.Banana, FoodHintText.ShakeOceanPalms);
            RegisterHint(ItemID.Coconut, FoodHintText.ShakeOceanPalms);
            RegisterHint(ItemID.Cherry, FoodHintText.ShakeSnowTrees);
            RegisterHint(ItemID.Plum, FoodHintText.ShakeSnowTrees);
            RegisterHint(ItemID.Mango, FoodHintText.ShakeJungleTrees);
            RegisterHint(ItemID.Pineapple, FoodHintText.ShakeJungleTrees);
            RegisterHint(ItemID.Pomegranate, FoodHintText.ShakeHellTrees);
            RegisterHint(ItemID.SpicyPepper, FoodHintText.ShakeHellTrees);
            RegisterHint(ItemID.Dragonfruit, FoodHintText.ShakeHolyTrees);
            RegisterHint(ItemID.Starfruit, FoodHintText.ShakeHolyTrees);

            var forestFruits = new int[] { ItemID.Apple, ItemID.Apricot, ItemID.Grapefruit, ItemID.Lemon, ItemID.Peach };
            RegisterHintMany(forestFruits, FoodHintText.ShakeForestTrees);
            var giftFood = new int[] { ItemID.ChristmasPudding, ItemID.GingerbreadCookie, ItemID.SugarCookie };
            RegisterHintMany(giftFood, FoodHintText.GiftBag);

            RegisterHint(ItemID.PadThai, FoodHintText.TravelingMerchant);
            CopyHintFrom(ItemID.PadThai, ItemID.Pho);
        }

        public override void OnWorldUnload()
        {
            FoodPool.Clear();
        }
        public static void RebuildFoodPool()
        {
            FoodPool.Clear();
            var set = new HashSet<int>();

            // 1) 先收“白名单里的原版食物”，再过滤“必须有配方”
            foreach (int type in VanillaFoodWhitelist)
                if (IsCraftableFood(type) && set.Add(type))
                    FoodPool.Add(type);

            // 2) 自动收集“所有被标记为食物的模组/原版物品（含你自己新增的）”，并过滤“必须有配方”
            for (int type = 1; type < ItemLoader.ItemCount; type++)
            {
                if (!ItemID.Sets.IsFood[type]) continue;
                if (!HasAnyRecipe(type)) continue;
                if (set.Add(type)) FoodPool.Add(type);
            }

            // 可选：排序（例如按 ID），避免联机时顺序不一致
            FoodPool.Sort();
        }

        private static bool IsCraftableFood(int type)
            => ItemID.Sets.IsFood[type] && HasAnyRecipe(type);

        private static bool HasAnyRecipe(int itemType)
        {
            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                var r = Main.recipe[i];
                if (r?.createItem?.type == itemType) return true;
            }
            return false;
        }

        // 取“玩家个人”的未合成池 = FoodPool - CraftedFoodTypes
        private static List<int> GetUncrafted(Player player)
        {
            var cp = player.GetModPlayer<CuisinePlayer>();
            var list = new List<int>(FoodPool.Count);
            foreach (int t in FoodPool)
                if (!cp.CraftedFoodTypes.Contains(t))
                    list.Add(t);
            return list;
        }

        // 每天显示两道（a,b）。若当天第一次取或换日，重置玩家的 TodayStartIndex = DayCounter % count
        public static void GetTodayTwo(Player player, out int first, out int second)
        {
            first = 0; second = 0;
            var cp = player.GetModPlayer<CuisinePlayer>();
            var uncrafted = GetUncrafted(player);
            int n = uncrafted.Count;
            if (n == 0) return;

            if (cp.TodayStartDay != DayCounter)
            {
                cp.TodayStartDay = DayCounter;
                cp.TodayStartIndex = DayCounter % n;
            }
            else
            {
                // 旧的一天：确保索引在范围内
                if (n > 0) cp.TodayStartIndex %= n;
            }

            first = uncrafted[cp.TodayStartIndex % n];
            second = uncrafted[(cp.TodayStartIndex + 1) % n];
        }

        // 制作其中一道后：立刻“补位”下一道
        // 规则：如果做的是 first → 不变更 TodayStartIndex（被移除后 second 自动顶上），
        //       如果做的是 second → TodayStartIndex++（first 保持，新增 next 顶到第二位）
        public static void OnCraftedAndRefresh(Player player, int craftedType)
        {
            var cp = player.GetModPlayer<CuisinePlayer>();
            var before = GetUncrafted(player);
            if (before.Count == 0) return;

            int first = before[cp.TodayStartIndex % before.Count];
            int second = before[(cp.TodayStartIndex + 1) % before.Count];

            // 先把成品加入“已合成”集合（外面也会加，这里幂等）
            cp.CraftedFoodTypes.Add(craftedType);

            // 计算补位
            var after = GetUncrafted(player); // 已移除 crafted 后的新池
            if (after.Count == 0) { cp.TodayStartIndex = 0; return; }

            if (craftedType == second)
            {
                // 做掉第二个：把窗口右移一位
                cp.TodayStartIndex = (cp.TodayStartIndex + 1) % after.Count;
            }
            // 做掉第一个：保持索引不变（原 second 顶到 first，新的 third 成为 second
        }
        // public static string DescribeAcquisition(int itemType)
        // {
        //     // ★ 1) 手动覆盖优先
        //     if (ManualFoodHints.TryGetValue(itemType, out string hint))
        //         return $"· {Lang.GetItemNameValue(itemType)}：{hint}";

        //     // 2) 有配方 → 列第一条配方（材料 + 合成站）
        //     for (int i = 0; i < Recipe.numRecipes; i++)
        //     {
        //         var r = Main.recipe[i];
        //         if (r?.createItem?.type == itemType)
        //         {
        //             var mats = new System.Text.StringBuilder();
        //             for (int k = 0; k < r.requiredItem.Count; k++)
        //             {
        //                 var it = r.requiredItem[k];
        //                 if (it?.type > 0 && it.stack > 0)
        //                 {
        //                     if (mats.Length > 0) mats.Append(" + ");
        //                     mats.Append($"{Lang.GetItemNameValue(it.type)}×{it.stack}");
        //                 }
        //             }

        //             // 合成站（取第一项），兼容 tML 1.4：用 tile id 直接取名
        //             string station = "";
        //             for (int t = 0; t < r.requiredTile.Count; t++)
        //             {
        //                 int tile = r.requiredTile[t];
        //                 if (tile >= 0 && tile < TileLoader.TileCount)
        //                 {
        //                     station = Lang.GetMapObjectName(tile);
        //                     if (!string.IsNullOrEmpty(station)) break;
        //                 }
        //             }
        //             if (string.IsNullOrEmpty(station)) station = "任意";
        //             return $"· {Lang.GetItemNameValue(itemType)}：{mats} @ {station}";
        //         }
        //     }
        //     // 3) 无配方 → 试图从原版掉落数据库推断 NPC 掉落；失败再给通用提示
        //     string drop = DescribeFromVanillaDropDB(itemType);
        //     if (!string.IsNullOrEmpty(drop))
        //         return $"· [i:{itemType}] {Lang.GetItemNameValue(itemType)}：{drop}";

        //     return $"未查询到配方";
        // }
        // CuisineSystem.cs 里（与 ManualFoodHints 同级）
        public static void RegisterHint(int itemType, string hint)
            => ManualFoodHints[itemType] = hint;

        public static void RegisterHintMany(IEnumerable<int> itemTypes, string hint)
        {
            foreach (var t in itemTypes)
                ManualFoodHints[t] = hint;
        }

        // 复用“某个物品”已写好的提示：把它拷贝给一组其它物品
        public static void CopyHintFrom(int sourceItemType, params int[] targetItemTypes)
        {
            if (!ManualFoodHints.TryGetValue(sourceItemType, out var hint)) return;
            foreach (var t in targetItemTypes)
                ManualFoodHints[t] = hint;
        }
        // 描述“从原版掉落数据库推断的来源（NPC）”
        private static string DescribeFromVanillaDropDB(int itemType, int maxNpc = 4)
        {
            // 1) 用 ItemDropDatabase 枚举所有 NPC 的掉落规则
            var db = Main.ItemDropsDB; // tML 版本不同字段名不同，二选一总有一个存在
            if (db is null) return ""; // 保险

            // 遍历 ContentSamples.NpcsByNetId（含负 netId 的事件怪）拿到各自规则列表
            List<(string npcName, float chance)> hits = new();
            foreach (var kv in ContentSamples.NpcsByNetId)
            {
                int npcNetId = kv.Key;
                var rules = db.GetRulesForNPCID(npcNetId, includeGlobalDrops: true);
                if (rules == null || rules.Count == 0) continue;

                // 2) 把规则树“展开”为 DropRateInfo（官方提供的递归汇总接口）
                var infos = new List<DropRateInfo>();
                var chain = new DropRateInfoChainFeed(1f);
                foreach (var r in rules)
                    r.ReportDroprates(infos, chain);

                // 3) 查找我们关心的 itemType 是否在该 NPC 的掉落中
                foreach (var info in infos)
                {
                    if (info.itemId == itemType)
                    {
                        // 记录 NPC 名称 + 掉率
                        string name = Lang.GetNPCNameValue(npcNetId);
                        hits.Add((name, info.dropRate)); // dropRate 是 0~1 的概率
                        break; // 该 NPC 已命中，不必重复计入
                    }
                }
            }

            if (hits.Count == 0) return "";

            // 4) 概率降序，裁掉过长列表
            hits.Sort((a, b) => b.chance.CompareTo(a.chance));
            if (hits.Count > maxNpc) hits = hits.GetRange(0, maxNpc);

            // 5) 组装提示
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < hits.Count; i++)
            {
                if (i > 0) sb.Append("、");
                // 格式示例：僵尸(2.0%)
                sb.Append($"{hits[i].npcName}({System.Math.Round(hits[i].chance * 100, 1)}%)");
            }
            return sb.ToString();
        }
        // 紧凑配方：返回例如 “配方： [i:ID]材料×数 + [i:ID]材料×数 @ 烹饪锅（共N种配方）”
        public static string DescribeRecipeCompact(int itemType, int maxMats = 6)
        {
            // 找到所有“创造该物品”的配方
            var matches = new List<Recipe>();
            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                var r = Main.recipe[i];
                if (r?.createItem?.type == itemType)
                    matches.Add(r);
            }
            if (matches.Count == 0)
            {
                // ★ 1) 手动覆盖优先
                if (ManualFoodHints.TryGetValue(itemType, out string hint))
                    return $"· 掉落： {hint}";
                // ★ 2) 无手动覆盖时，再从原版掉落数据库推断
                string drop = DescribeFromVanillaDropDB(itemType);
                if (!string.IsNullOrEmpty(drop))
                    return $"· 掉落： {drop}";
                else
                    return string.Empty;
            }

            // 只展示第一条，末尾标注“共 N 种配方”
            Recipe first = matches[0];
            var sb = new StringBuilder();
            sb.Append("· 配方： ");

            int shown = 0;
            for (int k = 0; k < first.requiredItem.Count; k++)
            {
                var it = first.requiredItem[k];
                if (it?.type > 0 && it.stack > 0)
                {
                    if (shown > 0) sb.Append(" + ");
                    sb.Append($"[i:{it.type}]{Lang.GetItemNameValue(it.type)}×{it.stack}");
                    shown++;
                    if (shown >= maxMats) { sb.Append(" + …"); break; }
                }
            }

            // 合成站（取第一个），无则“任意”
            string station = "任意";
            for (int t = 0; t < first.requiredTile.Count; t++)
            {
                int tile = first.requiredTile[t];
                if (tile >= 0 && tile < TileLoader.TileCount)
                {
                    string name = Lang.GetMapObjectName(tile);
                    if (!string.IsNullOrEmpty(name)) { station = name; break; }
                }
            }
            sb.Append($" @ {station}");

            if (matches.Count > 1)
                sb.Append($"（共{matches.Count}种配方）");

            return sb.ToString();
        }
    }

    public class CuisineGlobalItem : GlobalItem
    {
        // 更稳妥：制作瞬间直接扫描背包，而不是仅依赖 HasCookbook 标志
        private static bool HasCookbookNow(Player player)
        {
            for (int i = 0; i < player.inventory.Length; i++)
            {
                Item it = player.inventory[i];
                if (it?.ModItem is Cookbook) // 你的菜谱物品类名
                    return true;
            }
            return false;
        }
        public override void OnConsumeItem(Item item, Player player)
        {
            if (ItemID.Sets.IsFood[item.type])
            {
                CuisinePlayer p = player.GetModPlayer<CuisinePlayer>();
                if (p.FoodsEatenAll.Add(item.type))
                {
                    // 首次‘品尝’累加美味值
                    int bt = ContentSamples.ItemsByType[item.type].buffTime; // 帧
                    if (bt > 0) p.Deliciousness += bt;
                    CombatText.NewText(player.Hitbox, Color.Green, "品尝新食物");
                }
            }
        }

        public override void OnCreated(Item item, ItemCreationContext context)
        {
            var player = Main.LocalPlayer;
            var cp = player.GetModPlayer<CuisinePlayer>();

            bool hasCookbook = HasCookbookNow(player); // ✅ 以制作当帧的真实背包为准
            // —— 首次‘制作’累加厨艺值（与是否携带菜谱无关）——
            if (ItemID.Sets.IsFood[item.type] && cp.CraftedEverFoods.Add(item.type))
            {
                int bt = ContentSamples.ItemsByType[item.type].buffTime; // 单位：帧（60 = 1秒）
                if (bt > 0) cp.CookingSkill += bt;
            }
            // 仅当携带菜谱时才消耗‘首次双倍资格’与发奖励
            if (!hasCookbook || !ItemID.Sets.IsFood[item.type])
                return;

            // 今日两道（个人）
            CuisineSystem.GetTodayTwo(player, out int a, out int b);

            if (item.type == a || item.type == b)
            {
                player.QuickSpawnItem(player.GetSource_GiftOrReward(), item.type, item.stack);
                player.QuickSpawnItem(player.GetSource_GiftOrReward(), item.type, item.stack);
                CombatText.NewText(player.Hitbox, Color.Yellow, $"菜谱双倍奖励");
                cp.CraftedFoodTypes.Add(item.type);
                // —— 立刻补位（做掉第一道/第二道的规则在系统里处理）——
                CuisineSystem.OnCraftedAndRefresh(player, item.type);
            }
        }

    }
}