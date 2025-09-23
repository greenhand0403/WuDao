using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Global
{
    /// <summary>
    /// 处理“拾取萤火虫→20% 转化为春秋蝉、且春秋蝉限一只”的逻辑
    /// </summary>
    public class RewinderCicadasPickupGlobal : GlobalItem
    {
        public override bool InstancePerEntity => false;

        public override bool OnPickup(Item item, Player player)
        {
            int cicadaType = ModContent.ItemType<Items.Accessories.RewinderCicadas>();

            // —— 规则 1：若拾取的是萤火虫，20% 概率转化为春秋蝉（前提：当前不存在春秋蝉） —— //
            if (item.type == ItemID.Firefly && !HasCicadaAnywhere(player))
            {
                if (Main.rand.NextFloat() < 0.20f) // 20%
                {
                    // 移除这只萤火虫
                    if (item.active && item.whoAmI >= 0 && item.whoAmI < Main.maxItems)
                        Main.item[item.whoAmI].TurnToAir();

                    // 直接给玩家一只春秋蝉
                    if (player.whoAmI == Main.myPlayer)
                        player.QuickSpawnItem(player.GetSource_Misc("CicadaFromFirefly"), cicadaType, 1);

                    // 返回 false 表示“这个地面物品不进入背包”（我们已转化并发放）
                    return false;
                }
            }

            // —— 规则 2：若拾取的是春秋蝉，且身上任一位置已有一只 → 禁止再拾取 —— //
            if (item.type == cicadaType && HasCicadaAnywhere(player))
            {
                // 不准再拾，保持地面上这只不被吸走（玩家可以手动丢弃/别人捡）
                return false;
            }

            return base.OnPickup(item, player);
        }

        /// <summary>扫描“背包、四大钱箱/仓库、所有饰品栏（含扩展槽）”，判断是否已有春秋蝉</summary>
        public static bool HasCicadaAnywhere(Player player)
        {
            int cicada = ModContent.ItemType<Items.Accessories.RewinderCicadas>();

            // 背包（含快捷栏 & 钱币/弹药位在 inventory 里）
            for (int i = 0; i < player.inventory.Length; i++)
                if (!player.inventory[i].IsAir && player.inventory[i].type == cicada) return true;

            // 饰品栏（功能位）：armor[3..10 + extra]
            int start = 3;
            int lastInclusive = 3 + 7 + player.extraAccessorySlots;
            if (lastInclusive >= player.armor.Length) lastInclusive = player.armor.Length - 1;
            for (int i = start; i <= lastInclusive; i++)
                if (!player.armor[i].IsAir && player.armor[i].type == cicada) return true;

            // 四大“仓库”容器：Piggy Bank / Safe / Defender's Forge / Void Vault
            if (player.bank?.item != null)
                foreach (var it in player.bank.item) if (!it.IsAir && it.type == cicada) return true;
            if (player.bank2?.item != null)
                foreach (var it in player.bank2.item) if (!it.IsAir && it.type == cicada) return true;
            if (player.bank3?.item != null)
                foreach (var it in player.bank3.item) if (!it.IsAir && it.type == cicada) return true;
            if (player.bank4?.item != null)
                foreach (var it in player.bank4.item) if (!it.IsAir && it.type == cicada) return true;

            return false;
        }
    }
}
