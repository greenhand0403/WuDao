using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using WuDao.Common;
using WuDao.Content.Config;

namespace WuDao.Content.Global
{
    // 给盔甲“打标签”的全局组件（不是Prefix）
    public class ArmorElementTag : GlobalItem
    {
        // 每件物品各自一份数据
        public override bool InstancePerEntity => true;

        // -1=无，0=金，1=木，2=水，3=火，4=土
        public int Element = -1;

        public override void OnSpawn(Item item, IEntitySource source) => TryAssign(item);
        public override void OnCreated(Item item, ItemCreationContext context) => TryAssign(item);

        public override GlobalItem Clone(Item from, Item to)
        {
            var clone = (ArmorElementTag)base.Clone(from, to);
            clone.Element = Element;
            return clone;
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            if (Element >= 0) tag["ArmorElement"] = Element;
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            if (tag.ContainsKey("ArmorElement"))
                Element = tag.GetInt("ArmorElement");
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(Element);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            Element = reader.Read7BitEncodedInt();
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!IsArmor(item))
                return;

            // ① 显示“附带【X】属性”并着色（如果该盔甲有元素标签）
            if (Element >= 0)
            {
                string name = ElementName(Element);
                var line = new TooltipLine(Mod, "WuDaoArmorElement", $"【{name}】")
                {
                    OverrideColor = ElementColor(Element)
                };
                tooltips.Add(line);
            }

            // ② 若玩家当前激活了某个五行“三件套”，则在每个盔甲的 Tooltip 里追加一条“激活【X】三件套效果”
            int activeFullSet = GetActiveFullSetElement(Main.LocalPlayer);
            if (activeFullSet >= 0)
            {
                string actName = ElementName(activeFullSet);
                Color actColor = ElementColor(activeFullSet);

                // a) 在任意位置追加一条“激活”提示（让三件都能看见）
                // var activeLine = new TooltipLine(Mod, "WuDaoFullSetActive", $"五行【{actName}】效果")
                // {
                //     OverrideColor = actColor
                // };
                // tooltips.Add(activeLine);

                // b) 如果该物品的 Tooltip 里有“套装奖励(Set Bonus)”，就在它下面再插入一条“五行套装加成：……”
                int setBonusIndex = tooltips.FindIndex(t => t.Name == "SetBonus"); // 原版名称
                if (setBonusIndex != -1)
                {
                    string bonusDesc = FullSetExtraBonusText(activeFullSet);
                    var bonusLine = new TooltipLine(Mod, "WuDaoFullSetExtraBonus", $"盔甲五行：{bonusDesc}")
                    {
                        OverrideColor = actColor
                    };
                    tooltips.Insert(setBonusIndex + 1, bonusLine);
                }
            }
        }
        // 三件套“额外效果”的文字描述（显示在 Set Bonus 下面）
        private static string FullSetExtraBonusText(int e) => e switch
        {
            0 => "额外 +3% 伤害，护甲穿透 +6",
            1 => "额外 +3 生命再生，上限 +30%",
            2 => "额外 +4% 移速，加速度与减速度 +10%",
            3 => "额外 +2% 暴击率，暴击伤害 +10%",
            4 => "额外 +3 防御，耐力减伤 +10%",
            _ => ""
        };

        // 计算玩家是否激活了“五行三件套”，若激活返回元素索引（0..4），未激活返回 -1
        private static int GetActiveFullSetElement(Player player)
        {
            if (player == null) return -1;

            int[] counts = new int[5];
            Item[] armor = { player.armor[0], player.armor[1], player.armor[2] };
            foreach (var it in armor)
            {
                if (it == null || it.IsAir) continue;
                int e = it.GetGlobalItem<ArmorElementTag>()?.Element ?? -1;
                if (e >= 0 && e < 5) counts[e]++;
            }

            for (int e = 0; e < 5; e++)
                if (counts[e] == 3) return e;

            return -1;
        }
        public static Color ElementColor(int e) => e switch
        {
            0 => new Color(255, 215, 0),   // 金色
            1 => new Color(0, 200, 0),     // 绿色
            2 => new Color(30, 144, 255),  // 蓝色 (水)
            3 => new Color(220, 20, 60),   // 红色 (火)
            4 => new Color(139, 69, 19),   // 棕色 (土)
            _ => Color.White
        };
        public static string ElementName(int e) => e switch
        {
            0 => "金",
            1 => "木",
            2 => "水",
            3 => "火",
            4 => "土",
            _ => "无"
        };
        private void TryAssign(Item item)
        {
            if (!IsArmor(item) || Element >= 0) return; // 已有标签则不覆盖

            var cfg = ModContent.GetInstance<ArmorPrefixConfig>();
            if (cfg.PrefixMode == "禁用") return;
            if (cfg.PrefixMode == "仅矿物盔甲" && !IsVanillaOreArmor(item)) return;

            // 只在“获得盔甲时”随机赋值一次
            Element = Main.rand.Next(0, 5);

            // 可选：调试观察
            // Main.NewText($"获得盔甲：{Lang.GetItemNameValue(item.type)} → 元素={ElementName(Element)}");
        }

        private static bool IsArmor(Item item)
            => item != null && !item.IsAir && (item.headSlot >= 0 || item.bodySlot >= 0 || item.legSlot >= 0);

        private static bool IsVanillaOreArmor(Item item)
        {
            return ItemSets.OreArmorSet.Contains(item.type);
        }
    }
}
