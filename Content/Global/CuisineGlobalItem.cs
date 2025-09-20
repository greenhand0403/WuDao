using WuDao.Content.Systems;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Players;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using WuDao.Content.Items;
using System.Collections.Generic;
using WuDao.Content.Items.Weapons.Melee;

namespace WuDao.Content.Global
{
    /// <summary>
    /// “厨具 / 美食”集合及查询。
    /// 只要把物品 type 放到相应集合里，系统就会在伤害结算时给出加成。
    /// </summary>
    public static class CuisineCollections
    {
        /// <summary>厨具集合：会吃“厨艺值”乘区加成</summary>
        public static readonly HashSet<int> Cookware = new()
        {
            // 华夫饼烘烤模
            ItemID.WaffleIron,
            ModContent.ItemType<BlasterPliers>(),
        };

        /// <summary>美食集合：会吃“美味值”乘区加成</summary>
        public static readonly HashSet<int> Gourmet = new()
        {
            // 糖棒剑、火腿棍、蜂巢球、水果蛋糕旋刃、香蕉回旋镖、玉米糖步枪、星形茴香、（饰品）蜂巢
            ItemID.CandyCaneSword,
            ItemID.HamBat,
            ItemID.HiveFive,            // 蜂巢球（悠悠球）
            ItemID.FruitcakeChakram,    // 水果蛋糕旋刃
            ItemID.Bananarang,          // 香蕉回旋镖
            ItemID.CandyCornRifle,      // 玉米糖步枪
            ItemID.StarAnise,           // 星形茴香（投掷）
            ItemID.HoneyComb,           // 饰品蜂巢（本身无伤害，仅做分类展示/拓展用）
        };

        public static bool IsCookware(int type) => Cookware.Contains(type);
        public static bool IsGourmet(int type) => Gourmet.Contains(type);

        /// <summary>便捷注册（可在 Mod.Load 里调用动态扩充）</summary>
        public static void AddCookware(params int[] types) { foreach (var t in types) Cookware.Add(t); }
        public static void AddGourmet(params int[] types) { foreach (var t in types) Gourmet.Add(t); }
    }

    public class CuisineGlobalItem : GlobalItem
    {
        // 每点数值折算为多少“额外倍数”。举例：0.01 => 100 点 = +100% = ×2
        // 你可以按自己的数值膨胀度调这两个常量。
        public const float PerCookingPointToBonus = 0.01f;
        public const float PerDeliciousPointToBonus = 0.01f;

        // “最高 300%”→ 额外倍数上限为 +3.0（最终乘区为 1 + 3 = ×4）
        public const float MaxExtraMultiplier = 3f;
        // ① 放在 CuisineGlobalItem 类里常量区（和 MaxExtraMultiplier 等并列）
        public const int MaxGourmetDefenseBonus = 30; // “满 300%”时的最大额外防御；可按需调

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
                    // 触发食物海事件
                    FoodRainSystem.TryTrigger(player);
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
        // 把“厨艺值 / 美味值”转成伤害乘区（上限 +300%）
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            var cp = player.GetModPlayer<CuisinePlayer>();

            // 厨具 → 厨艺值乘区
            if (CuisineCollections.IsCookware(item.type))
            {
                float extra = MathHelper.Clamp(cp.CookingSkill * PerCookingPointToBonus, 0f, MaxExtraMultiplier);
                damage *= (1f + extra);
            }

            // 美食 → 美味值乘区
            if (CuisineCollections.IsGourmet(item.type))
            {
                float extra = MathHelper.Clamp(cp.Deliciousness * PerDeliciousPointToBonus, 0f, MaxExtraMultiplier);
                damage *= (1f + extra);
            }
        }
        // ② 覆写 UpdateAccessory：仅当该物品是饰品且属于“美食”集合时，按美味值加防
        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (!item.accessory) return; // 只处理饰品
            if (!CuisineCollections.IsGourmet(item.type)) return;

            var cp = player.GetModPlayer<CuisinePlayer>();

            // 复用你现有的美味值→倍率逻辑，最高 +300%（extra ∈ [0, 3]）
            float extra = MathHelper.Clamp(cp.Deliciousness * PerDeliciousPointToBonus, 0f, MaxExtraMultiplier);

            // 把“进度”线性映射成防御值（满 300% 时给 +MaxGourmetDefenseBonus）
            int defBonus = (int)System.Math.Round(MaxGourmetDefenseBonus * (extra / MaxExtraMultiplier));

            if (defBonus > 0)
                player.statDefense += defBonus; // 给到最终防御
        }

        // 仅做标记展示（可选）
        public override void ModifyTooltips(Item item, List<Terraria.ModLoader.TooltipLine> tooltips)
        {
            if (CuisineCollections.IsCookware(item.type))
                tooltips.Add(new TooltipLine(Mod, "CuisineTag", "厨具"));
            if (CuisineCollections.IsGourmet(item.type))
                tooltips.Add(new TooltipLine(Mod, "CuisineTag", "美食"));
            // 在你已有的 ModifyTooltips 里追加这一段判断后缀（放在“美食”判断后即可）
            if (item.accessory && CuisineCollections.IsGourmet(item.type))
            {
                var cp = Main.LocalPlayer?.GetModPlayer<CuisinePlayer>();
                if (cp != null)
                {
                    float extra = MathHelper.Clamp(cp.Deliciousness * PerDeliciousPointToBonus, 0f, MaxExtraMultiplier);
                    int defBonus = (int)System.Math.Round(MaxGourmetDefenseBonus * (extra / MaxExtraMultiplier));
                    tooltips.Add(new TooltipLine(Mod, "CuisineDefense",
                        $"美味饰品加成：+{defBonus} 防御（随美味值，提高至最高 +{MaxGourmetDefenseBonus}）"));
                }
            }
        }

        // 说明：饰品一般无直接伤害，这里不做处理。
        // 若你以后做“会发射弹幕的饰品/召唤类饰品”，其伤害会在生成时读取玩家面板，
        // 上面 ModifyWeaponDamage 的乘区已能覆盖到。
        public override bool InstancePerEntity => false;
    }
}