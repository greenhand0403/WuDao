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
    public class JuexueRightClickGlobal : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
            => entity.ModItem is JuexueItem;

        public override bool CanRightClick(Item item) => true;

        public override void RightClick(Item item, Player player)
        {
            var qi = player.GetModPlayer<QiPlayer>();
            SoundEngine.PlaySound(SoundID.Grab, player.Center);

            int idx = -1;
            for (int i = 0; i < player.inventory.Length; i++)
            {
                if (ReferenceEquals(player.inventory[i], item))
                {
                    idx = i;
                    break;
                }
            }

            if (!JuexueRuntime.Enabled)
            {
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText(Mod.GetLocalization("Mods.WuDao.Messages.JueXue.Config"), Color.OrangeRed);

                HandleFailedEquip(player, item, idx);
                return;
            }

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

            bool fav = item.favorited;

            if (qi.JuexueSlot.IsAir)
            {
                qi.JuexueSlot = item.Clone();
                if (idx != -1)
                    player.inventory[idx].TurnToAir();
            }
            else
            {
                Item old = qi.JuexueSlot.Clone();
                old.favorited = fav;

                qi.JuexueSlot = item.Clone();

                if (idx != -1)
                {
                    player.inventory[idx] = old;
                }
                else
                {
                    if (player.ItemSpace(old).CanTakeItemToPersonalInventory)
                        player.GetItem(player.whoAmI, old, GetItemSettings.InventoryEntityToPlayerInventorySettings);
                    else
                        Item.NewItem(player.GetSource_Misc("QiSwapDrop"), player.getRect(), old);
                }
            }

            qi.RequestSyncJuexueSlot();

            if (!Main.mouseItem.IsAir)
                Main.mouseItem.TurnToAir();

            player.mouseInterface = true;
            Main.mouseRightRelease = false;
            Main.stackSplit = 9999;
        }

        private void HandleFailedEquip(Player player, Item item, int inventoryIndex)
        {
            SoundEngine.PlaySound(SoundID.MenuClose, player.Center);
            if (inventoryIndex >= 0)
                player.inventory[inventoryIndex] = item.Clone();

            player.mouseInterface = true;
            Main.mouseRightRelease = false;
        }
    }
}