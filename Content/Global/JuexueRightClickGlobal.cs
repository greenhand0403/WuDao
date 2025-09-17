using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;

namespace WuDao.Content.Juexue
{
    public class JuexueRightClickGlobal : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
            => entity.ModItem is JuexueItem;

        public override bool CanRightClick(Item item) => true;

        public override void RightClick(Item item, Player player)
        {
            var qi = player.GetModPlayer<QiPlayer>();
            SoundEngine.PlaySound(SoundID.Grab, player.Center);

            // 找到“这件被右键的物品”所在的背包槽位索引
            int idx = -1;
            for (int i = 0; i < player.inventory.Length; i++)
                if (object.ReferenceEquals(player.inventory[i], item)) { idx = i; break; }

            bool fav = item.favorited; // 记住收藏星标（可选，让星标留在原格）

            if (qi.JuexueSlot.IsAir)
            {
                // 槽位空：装备当前这件，并把原格清空
                qi.JuexueSlot = item.Clone();

                if (idx != -1)
                    player.inventory[idx].TurnToAir();
            }
            else
            {
                // 槽位已有：交换
                Item old = qi.JuexueSlot.Clone();
                old.favorited = fav; // 星标沿用（可选）

                qi.JuexueSlot = item.Clone();

                if (idx != -1)
                {
                    // 关键：把被替换下来的“旧绝学”直接写回到右键的那个背包格
                    player.inventory[idx] = old;
                }
                else
                {
                    // 异常兜底（比如右键的是箱子里的物品，不在个人背包数组里）
                    if (player.ItemSpace(old).CanTakeItemToPersonalInventory)
                        player.GetItem(player.whoAmI, old, GetItemSettings.InventoryEntityToPlayerInventorySettings);
                    else
                        Item.NewItem(player.GetSource_Misc("QiSwapDrop"), player.getRect(), old);
                }
            }

            // 结束状态：不让鼠标拿任何东西
            if (!Main.mouseItem.IsAir)
                Main.mouseItem.TurnToAir();

            // 阻断本帧继续交互，避免默认逻辑再“拿起/分堆”
            player.mouseInterface = true;
            Main.mouseRightRelease = false;
            Main.stackSplit = 9999; // 防止右键连击分堆干扰（稳一点）
        }
    }
}
