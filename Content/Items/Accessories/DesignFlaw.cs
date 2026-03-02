using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using WuDao.Content.Players;
using Terraria.Localization;

namespace WuDao.Content.Items.Accessories
{
    /*
        败笔，核心机制：
        记录击败玩家的 NPC 类型。
        玩家死亡时，如果是被 NPC 杀死 → 记录该 NPC 类型和次数。
        装备后，对该 NPC 造成 10% × 被击败次数 的额外伤害。
        击杀该 NPC 时，额外掉落，并清空记录。
    */
    // TODO: 中文提示
    /// <summary>
    /// 败笔 饰品
    /// </summary>
    public class DesignFlaw : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DesignFlawPlayer>().hasFlaw = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 如果你要替换或追加 tooltip，use Language.GetTextValue 或 Language.GetText(...).Format(...)

            // 再追加一行额外提示（示例：显示被记录的 NPC 名称与次数 - 假设你从 player 数据里读到）
            var modPlayer = Main.LocalPlayer.GetModPlayer<DesignFlawPlayer>(); // 举例，替换为你实际 player 类
            if (modPlayer != null && modPlayer.recordedNPCType > 0)
            {
                // 假设你要显示 "记录：{npcName} ×{count}"，先从 key 取带占位符的文本，然后 Format
                LocalizedText lt = Language.GetText("Mods.WuDao.Items.DesignFlaw.TooltipRecorded");
                string npcName = Lang.GetNPCNameValue(modPlayer.recordedNPCType);
                if (string.IsNullOrEmpty(npcName))
                    npcName = "??"; // fallback
                string formatted = lt.Format(npcName, modPlayer.defeatCount);
                tooltips.Add(new TooltipLine(Mod, "DesignFlawRecorded", formatted));
            }
            else
            {
                // 没有记录时显示另一条本地化提示（可选）
                string none = Language.GetTextValue("Mods.WuDao.Items.DesignFlaw.TooltipNoRecord");
                if (!string.IsNullOrEmpty(none))
                    tooltips.Add(new TooltipLine(Mod, "DesignFlawNoRecord", none));
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BlackInk)
                .AddIngredient(ItemID.Feather)
                .AddIngredient(ItemID.BambooBlock)
                .AddIngredient(ItemID.ManaCrystal)
                .AddIngredient(ItemID.Book)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
