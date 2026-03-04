
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using Microsoft.Xna.Framework;
using WuDao.Content.Config;

namespace WuDao.Content.Global
{
    // 右键自动装备绝学到绝学栏的物品
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
            {
                if (ReferenceEquals(player.inventory[i], item))
                {
                    idx = i; break;
                }
            }

            // 未启用绝学系统时，不处理绝学书籍的右键自动装备
            if (!ModContent.GetInstance<WudaoConfig>().EnableJueXueSystem)
            {
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText(Mod.GetLocalization("Mods.WuDao.Messages.JueXue.Config"), Color.OrangeRed);

                HandleFailedEquip(player, item, idx);
                return;
            }
            // 仅当“当前槽位是主动绝学且仍在冷却”时，禁止更换
            if (!qi.JuexueSlot.IsAir
                && qi.JuexueSlot.ModItem is JuexueItem cur
                && cur.IsActive
                && !qi.CanUseActiveNow(qi.JuexueSlot.type, cur.SpecialCooldownTicks))
            {
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText(Mod.GetLocalization("Mods.WuDao.Messages.JueXue.Cooldown"), Color.OrangeRed);

                HandleFailedEquip(player, item, idx);
                return;
            }

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
        // 处理装备失败的情况
        private void HandleFailedEquip(Player player, Item item, int inventoryIndex)
        {
            // 注意：这里千万别动 item / Main.mouseItem / 背包数据
            SoundEngine.PlaySound(SoundID.MenuClose, player.Center);
            // 默认当作“被消费了一次”，无法替换新绝学时，需要把当前物品克隆一份放回原位
            player.inventory[inventoryIndex] = item.Clone();
            // 防止这次右键继续被处理
            player.mouseInterface = true;
            Main.mouseRightRelease = false;
        }
    }
}
