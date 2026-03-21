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
using System;
using WuDao.Content.Items.Accessories;
using Terraria.Localization;

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
            // 海虎爆破钳
            ModContent.ItemType<BlasterPliers>(),
            // 葱剑
            ModContent.ItemType<ScallionSword>(),
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
            ModContent.ItemType<ScallionShield>(),//葱盾
        };
        /// <summary>
        /// 查询是否为厨具武器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsCookware(int type) => Cookware.Contains(type);
        /// <summary>
        /// 查询是否为美食武器或饰品
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsGourmet(int type) => Gourmet.Contains(type);

        /// <summary>便捷注册（可在 Mod.Load 里调用动态扩充）</summary>
        // public static void AddCookware(params int[] types) { foreach (var t in types) Cookware.Add(t); }
        // public static void AddGourmet(params int[] types) { foreach (var t in types) Gourmet.Add(t); }
    }

    public class CuisineGlobalItem : GlobalItem
    {
        // 每点数值折算为多少“额外倍数” 原版大约有31种菜85种食物
        public const float PerCookingPointToBonus = 0.1f;
        public const float PerDeliciousPointToBonus = 0.05f;
        public override bool InstancePerEntity => false;
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
        private static void SendCuisineFoodRainRequest(Player player, Mod mod)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient || player.whoAmI != Main.myPlayer)
                return;

            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestCuisineFoodRain);
            packet.Write((byte)player.whoAmI);
            packet.Send();
        }

        private static void SendCuisineCraftRewardRequest(Player player, Item item, Mod mod)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient || player.whoAmI != Main.myPlayer)
                return;

            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestCuisineCraftReward);
            packet.Write((byte)player.whoAmI);
            packet.Write(item.type);
            packet.Write(item.stack);
            packet.Send();
        }
        public override void OnConsumeItem(Item item, Player player)
        {
            if (!ItemID.Sets.IsFood[item.type])
                return;

            // 多人里由“拥有该客户端的玩家”本地管理菜单与首次品尝，再同步结果给服务器
            if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer)
                return;

            CuisinePlayer p = player.GetModPlayer<CuisinePlayer>();

            bool wasNewForMenu = p.FoodsEatenAll.Add(item.type);
            bool wasFirstTaste = p.EatenEverFoods.Add(item.type);

            if (wasNewForMenu && p.HasFoodLogItem)
                p.RefreshSuggestedFoods6();

            if (!wasFirstTaste)
                return;

            int bt = ContentSamples.ItemsByType[item.type].rare;
            if (bt > 0)
                p.Deliciousness += bt;

            p.MarkCuisineDirty();

            CombatText.NewText(
                player.Hitbox,
                Color.Green,
                Language.GetTextValue("Mods.WuDao.Cuisine.Messages.TastedNewFood")
            );

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                FoodRainSystem.TryTrigger(player);
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer)
            {
                SendCuisineFoodRainRequest(player, Mod);
            }
        }

        public override void OnCreated(Item item, ItemCreationContext context)
        {
            if (!ItemID.Sets.IsFood[item.type])
                return;

            // 多人里这套菜单/菜谱逻辑只由本地客户端自己管理
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                Player localPlayer = Main.LocalPlayer;
                if (localPlayer == null || !localPlayer.active)
                    return;

                var cpLocal = localPlayer.GetModPlayer<CuisinePlayer>();

                // 首次制作 -> 厨艺值
                if (cpLocal.CraftedEverFoods.Add(item.type))
                {
                    int bt = ContentSamples.ItemsByType[item.type].rare;
                    if (bt > 0)
                        cpLocal.CookingSkill += bt;

                    cpLocal.MarkCuisineDirty();
                }

                bool hasCookbookLocal = HasCookbookNow(localPlayer);
                if (!hasCookbookLocal)
                    return;

                CuisineSystem.GetTodayTwo(localPlayer, out int aLocal, out int bLocal);

                if (item.type == aLocal || item.type == bLocal)
                {
                    CombatText.NewText(
                        localPlayer.Hitbox,
                        Color.Yellow,
                        Language.GetTextValue("Mods.WuDao.Cuisine.Messages.CookbookDoubleReward")
                    );

                    cpLocal.CraftedFoodTypes.Add(item.type);
                    CuisineSystem.OnCraftedAndRefresh(localPlayer, item.type);

                    SendCuisineCraftRewardRequest(localPlayer, item, Mod);
                }

                return;
            }

            // 单机：直接生效
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                var player = Main.LocalPlayer;
                if (player == null || !player.active)
                    return;

                var cp = player.GetModPlayer<CuisinePlayer>();

                if (cp.CraftedEverFoods.Add(item.type))
                {
                    int bt = ContentSamples.ItemsByType[item.type].rare;
                    if (bt > 0)
                        cp.CookingSkill += bt;
                }

                bool hasCookbook = HasCookbookNow(player);
                if (!hasCookbook)
                    return;

                CuisineSystem.GetTodayTwo(player, out int a, out int b);

                if (item.type == a || item.type == b)
                {
                    player.QuickSpawnItem(player.GetSource_GiftOrReward(), item.type, item.stack);
                    player.QuickSpawnItem(player.GetSource_GiftOrReward(), item.type, item.stack);

                    CombatText.NewText(
                        player.Hitbox,
                        Color.Yellow,
                        Language.GetTextValue("Mods.WuDao.Cuisine.Messages.CookbookDoubleReward")
                    );

                    cp.CraftedFoodTypes.Add(item.type);
                    CuisineSystem.OnCraftedAndRefresh(player, item.type);
                }

                return;
            }

            // 专服：菜单和菜谱是客户端本地权威，这里不做本地菜单判定
            if (Main.netMode == NetmodeID.Server)
                return;
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
            int defBonus = (int)Math.Round(MaxGourmetDefenseBonus * (extra / MaxExtraMultiplier));

            if (defBonus > 0)
                player.statDefense += defBonus; // 给到最终防御
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            var cp = Main.LocalPlayer?.GetModPlayer<CuisinePlayer>();
            // 厨具武器加成
            if (CuisineCollections.IsCookware(item.type))
            {
                // 放在物品名字的下一行
                var line = new TooltipLine(
                    Mod, "CuisineTag",
                    Language.GetTextValue("Mods.WuDao.Cuisine.Tooltip.Tag.Cookware")
                );
                int nameIndex = tooltips.FindIndex(t => t.Name == "ItemName");

                if (nameIndex != -1)
                {
                    tooltips.Insert(nameIndex + 1, line); // 插到名字下面
                }
                else
                {
                    tooltips.Insert(0, line); // fallback
                }

                if (cp != null)
                {
                    float extra = MathHelper.Clamp(cp.CookingSkill * PerCookingPointToBonus, 0f, MaxExtraMultiplier);
                    int percent = (int)(extra * 100);
                    int maxPercent = (int)(MaxExtraMultiplier * 100);

                    tooltips.Add(new TooltipLine(
                        Mod, "CuisineBonus",
                        Language.GetTextValue("Mods.WuDao.Cuisine.Tooltip.CookwareDamageBonus", percent, maxPercent)
                    ));
                }
            }
            // 美食武器加成
            if (CuisineCollections.IsGourmet(item.type))
            {
                // 放在物品名字的下一行
                var line = new TooltipLine(
                    Mod, "CuisineTag",
                    Language.GetTextValue("Mods.WuDao.Cuisine.Tooltip.Tag.Gourmet")
                );
                int nameIndex = tooltips.FindIndex(t => t.Name == "ItemName");

                if (nameIndex != -1)
                {
                    tooltips.Insert(nameIndex + 1, line); // 插到名字下面
                }
                else
                {
                    tooltips.Insert(0, line); // fallback
                }

                if (cp != null)
                {
                    float extra = MathHelper.Clamp(cp.Deliciousness * PerDeliciousPointToBonus, 0f, MaxExtraMultiplier);
                    int percent = (int)(extra * 100);
                    int maxPercent = (int)(MaxExtraMultiplier * 100);

                    tooltips.Add(new TooltipLine(
                        Mod, "CuisineBonus",
                        Language.GetTextValue("Mods.WuDao.Cuisine.Tooltip.GourmetDamageBonus", percent, maxPercent)
                    ));
                }
            }

            // 保留你原本的美食饰品加防御逻辑
            if (item.accessory && CuisineCollections.IsGourmet(item.type) && cp != null)
            {
                float extra = MathHelper.Clamp(cp.Deliciousness * PerDeliciousPointToBonus, 0f, MaxExtraMultiplier);
                int defBonus = (int)Math.Round(MaxGourmetDefenseBonus * (extra / MaxExtraMultiplier));
                tooltips.Add(new TooltipLine(
                    Mod, "CuisineDefense",
                    Language.GetTextValue("Mods.WuDao.Cuisine.Tooltip.GourmetAccessoryDefenseBonus", defBonus, MaxGourmetDefenseBonus)
                ));
            }
        }

    }
}