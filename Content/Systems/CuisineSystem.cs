using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using WuDao.Content.Players;
using Terraria.GameContent.ItemDropRules;
using System.Text;
using Terraria.Localization;

namespace WuDao.Content.Systems
{
    /// <summary>
    /// 菜谱厨艺值系统
    /// </summary>
    public class CuisineSystem : ModSystem
    {
        public static class FoodHintText
        {
            public const string TravelingMerchantInSnow = "Mods.WuDao.Cuisine.Hints.TravelingMerchantInSnow";
            public const string TravelingMerchant = "Mods.WuDao.Cuisine.Hints.TravelingMerchant";
            public const string Fishing = "Mods.WuDao.Cuisine.Hints.Fishing";
            public const string ShakeForestTrees = "Mods.WuDao.Cuisine.Hints.ShakeForestTrees";
            public const string ShakeOceanPalms = "Mods.WuDao.Cuisine.Hints.ShakeOceanPalms";
            public const string ShakeSnowTrees = "Mods.WuDao.Cuisine.Hints.ShakeSnowTrees";
            public const string ShakeJungleTrees = "Mods.WuDao.Cuisine.Hints.ShakeJungleTrees";
            public const string ShakeHellTrees = "Mods.WuDao.Cuisine.Hints.ShakeHellTrees";
            public const string ShakeHolyTrees = "Mods.WuDao.Cuisine.Hints.ShakeHolyTrees";
            public const string GiftBag = "Mods.WuDao.Cuisine.Hints.GiftBag";
            public const string Chest = "Mods.WuDao.Cuisine.Hints.Chest";
            public const string BoughtFromNPC = "Mods.WuDao.Cuisine.Hints.BoughtFromNPC";

            // 下面这些原来是直接写中文的特殊提示，也改成 key
            public const string ShakeEvilTrees = "Mods.WuDao.Cuisine.Hints.ShakeEvilTrees";
            public const string ShakeCrimsonTrees = "Mods.WuDao.Cuisine.Hints.ShakeCrimsonTrees";
            public const string ShuckedOyster = "Mods.WuDao.Cuisine.Hints.ShuckedOyster";
            public const string CookedMarshmallow = "Mods.WuDao.Cuisine.Hints.CookedMarshmallow";
        }

        private static readonly int[] VanillaFoodWhitelist = new int[] {
            ItemID.GoldenDelight,
            ItemID.PrismaticPunch,
            ItemID.FruitSalad,
            ItemID.SeafoodDinner,
            ItemID.GrubSoup,
            ItemID.TropicalSmoothie,
            ItemID.SmoothieofDarkness,
            ItemID.PinaColada,
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
            ManualFoodHints[ItemID.Elderberry] = FoodHintText.ShakeEvilTrees;
            ManualFoodHints[ItemID.BlackCurrant] = FoodHintText.ShakeEvilTrees;
            ManualFoodHints[ItemID.BloodOrange] = FoodHintText.ShakeCrimsonTrees;
            ManualFoodHints[ItemID.Rambutan] = FoodHintText.ShakeCrimsonTrees;
            ManualFoodHints[ItemID.ShuckedOyster] = FoodHintText.ShuckedOyster;

            RegisterHint(ItemID.CookedMarshmallow, FoodHintText.CookedMarshmallow);
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
            var sb = new StringBuilder();
            // for (int i = 0; i < hits.Count; i++)
            // {
            //     if (i > 0) sb.Append("、");
            //     // 格式示例：僵尸(2.0%)
            //     sb.Append($"{hits[i].npcName}({Math.Round(hits[i].chance * 100, 1)}%)");
            // }
            string sep = Language.GetTextValue("Mods.WuDao.Cuisine.Drop.Separator");
            for (int i = 0; i < hits.Count; i++)
            {
                if (i > 0) sb.Append(sep);

                double pct = Math.Round(hits[i].chance * 100, 1);
                sb.Append(Language.GetTextValue("Mods.WuDao.Cuisine.Drop.Entry", hits[i].npcName, pct));
            }
            return sb.ToString();
        }
        private static string L(string keyOrText)
        {
            // 我们约定：以 "Mods." 开头的是本地化 key，否则当作普通文本原样返回
            return keyOrText != null && keyOrText.StartsWith("Mods.", StringComparison.Ordinal)
                ? Language.GetTextValue(keyOrText)
                : keyOrText ?? string.Empty;
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
                if (ManualFoodHints.TryGetValue(itemType, out string hintKey))
                    return Language.GetTextValue("Mods.WuDao.Cuisine.Recipe.BulletDrop", L(hintKey));
                // ★ 2) 无手动覆盖时，再从原版掉落数据库推断
                string drop = DescribeFromVanillaDropDB(itemType);
                if (!string.IsNullOrEmpty(drop))
                    return Language.GetTextValue("Mods.WuDao.Cuisine.Recipe.BulletDrop", drop);
                else
                    return string.Empty;
            }

            // 只展示第一条，末尾标注“共 N 种配方”
            Recipe first = matches[0];
            var sb = new StringBuilder();

            // 有配方时：
            sb.Append(Language.GetTextValue("Mods.WuDao.Cuisine.Recipe.BulletRecipePrefix"));

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

            // 合成站（取第一个），无则显示"无"
            string station = Language.GetTextValue("LegacyMisc.23");

            for (int t = 0; t < first.requiredTile.Count; t++)
            {
                int tile = first.requiredTile[t];
                if (tile >= 0 && tile < TileLoader.TileCount)
                {
                    string name = Lang.GetMapObjectName(tile);
                    if (!string.IsNullOrEmpty(name)) { station = name; break; }
                }
            }
            sb.Append(Language.GetTextValue("Mods.WuDao.Cuisine.Recipe.StationSuffix", station));

            if (matches.Count > 1)
                sb.Append(Language.GetTextValue("Mods.WuDao.Cuisine.Recipe.MultiRecipeSuffix", matches.Count));

            return sb.ToString();
        }
    }
}