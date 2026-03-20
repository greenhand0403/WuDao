using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using WuDao.Common;
using WuDao.Content.Config;
using static WuDao.Content.Config.ArmorPrefixConfig;

namespace WuDao.Content.Global
{
    // 盔甲五行机制，给盔甲“打标签”的全局组件（不是Prefix）
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
                string name = ElementName(Element); // 下面我们会把 ElementName 改成“本地化版本”

                // 紧随物品名字
                int nameIndex = tooltips.FindIndex(t => t.Name == "ItemName" && t.Mod == "Terraria");

                if (nameIndex != -1)
                {
                    var nameLine = tooltips[nameIndex];

                    Color c = ElementColor(Element);
                    string hex = $"{c.R:X2}{c.G:X2}{c.B:X2}";

                    nameLine.Text += $" [c/{hex}:【{name}】]";
                }
            }

            // ② 若玩家当前激活了某个五行“三件套”，则在每个盔甲的 Tooltip 里追加一条“激活【X】三件套效果”
            int activeFullSet = GetActiveFullSetElement(Main.LocalPlayer);
            if (activeFullSet >= 0)
            {
                string actName = ElementName(activeFullSet);
                Color actColor = ElementColor(activeFullSet);
                // b) 如果该物品的 Tooltip 里有“套装奖励(Set Bonus)”，就在它下面再插入一条“五行套装加成：……”
                int setBonusIndex = tooltips.FindIndex(t => t.Name == "SetBonus"); // 原版名称
                if (setBonusIndex != -1)
                {
                    string bonusDesc = FullSetExtraBonusText(activeFullSet);
                    var bonusLine = new TooltipLine(
                        Mod,
                        "WuDaoFullSetExtraBonus",
                        Language.GetTextValue("Mods.WuDao.ArmorElement.Tooltip.SetBonusPrefix", bonusDesc) // "盔甲五行：{0}"
                    )
                    {
                        OverrideColor = actColor
                    };
                    tooltips.Insert(setBonusIndex + 1, bonusLine);
                }
            }
        }
        private static string FullSetExtraBonusText(int e) => e switch
        {
            0 => Language.GetTextValue("Mods.WuDao.ArmorElement.SetBonus.Metal"),
            1 => Language.GetTextValue("Mods.WuDao.ArmorElement.SetBonus.Wood"),
            2 => Language.GetTextValue("Mods.WuDao.ArmorElement.SetBonus.Water"),
            3 => Language.GetTextValue("Mods.WuDao.ArmorElement.SetBonus.Fire"),
            4 => Language.GetTextValue("Mods.WuDao.ArmorElement.SetBonus.Earth"),
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
            0 => Language.GetTextValue("Mods.WuDao.ArmorElement.Name.Metal"),
            1 => Language.GetTextValue("Mods.WuDao.ArmorElement.Name.Wood"),
            2 => Language.GetTextValue("Mods.WuDao.ArmorElement.Name.Water"),
            3 => Language.GetTextValue("Mods.WuDao.ArmorElement.Name.Fire"),
            4 => Language.GetTextValue("Mods.WuDao.ArmorElement.Name.Earth"),
            _ => Language.GetTextValue("Mods.WuDao.ArmorElement.Name.None")
        };
        private void TryAssign(Item item)
        {
            if (item == null || item.IsAir)
                return;

            if (Element >= 0)
                return;

            if (!IsArmor(item))
                return;

            // 多人里客户端不负责决定真实元素
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            var cfg = ModContent.GetInstance<ArmorPrefixConfig>();
            if (cfg == null)
                return;

            if (cfg.PrefixMode == ArmorPrefixMode.Disabled)
                return;

            if (cfg.PrefixMode == ArmorPrefixMode.OreArmorOnly && !IsVanillaOreArmor(item))
                return;

            Element = Main.rand.Next(0, 5);
        }

        private static bool IsArmor(Item item)
            => item != null && !item.IsAir && (item.headSlot >= 0 || item.bodySlot >= 0 || item.legSlot >= 0);

        private static bool IsVanillaOreArmor(Item item)
        {
            return ItemSets.OreArmorSet.Contains(item.type);
        }
    }
}
