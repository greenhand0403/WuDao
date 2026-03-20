using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Localization;

namespace WuDao.Content.Development
{
    // 开局礼包掉落武道模组所有物品
    public enum BundleCategory
    {
        Weapons,
        Accessories,
        Others
    }

    public class WeaponBundleItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 5;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.rare = ItemRarityID.Red;
            Item.consumable = false;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player)
        {
            BundleSelectSystem.Show(category =>
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    GiveItemsForCategoryNetSafe(player, category, Mod);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)MessageType.SelectBundleCategory);
                    packet.Write((byte)player.whoAmI);
                    packet.Write((byte)category);
                    packet.Send();
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    GiveItemsForCategoryNetSafe(player, category, Mod);
                }
            });

            Main.playerInventory = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        public static void GiveItemsForCategoryNetSafe(Player player, BundleCategory category, Mod mod)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return; // 客户端禁止真正发物品

            int addedKinds = 0;
            int droppedKinds = 0;
            int droppedTotal = 0;
            var src = player.GetSource_Misc("WuDaoBundle");

            for (int type = 1; type < ItemLoader.ItemCount; type++)
            {
                Item probe = new Item();
                probe.SetDefaults(type);

                if (probe.IsAir) continue;
                if (probe.ModItem?.Mod != mod) continue;
                if (type == ModContent.ItemType<WeaponBundleItem>()) continue;

                if (!BelongsToCategory(probe, category))
                    continue;

                int stack = probe.maxStack > 1 ? probe.maxStack : 1;

                Item give = new Item();
                give.SetDefaults(type);
                give.stack = stack;

                Item leftover = player.GetItem(player.whoAmI, give, GetItemSettings.LootAllSettings);

                if (leftover != null && leftover.stack > 0)
                {
                    Item.NewItem(src, player.getRect(), leftover.type, leftover.stack);
                    droppedKinds++;
                    droppedTotal += leftover.stack;
                }
                else
                {
                    addedKinds++;
                }
            }

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(
                    Language.GetTextValue("Mods.WuDao.Messages.BundleClaimed", addedKinds, droppedKinds, droppedTotal),
                    255, 240, 20
                );
            }

            if (Main.netMode == NetmodeID.Server && player.whoAmI >= 0)
            {
                Terraria.Chat.ChatHelper.SendChatMessageToClient(
                    NetworkText.FromLiteral(
                        Language.GetTextValue("Mods.WuDao.Messages.BundleClaimed", addedKinds, droppedKinds, droppedTotal)
                    ),
                    Microsoft.Xna.Framework.Color.Yellow,
                    player.whoAmI
                );
            }
        }
        private static bool BelongsToCategory(Item it, BundleCategory cat)
        {
            switch (cat)
            {
                case BundleCategory.Weapons:
                    // 定义：所有有伤害的物品（近战/远程/魔法/召唤/鞭/投掷等）或“弹药”
                    // （排除纯工具：可按需把镐/斧/锤排除；若你希望工具也算武器，可删掉这些判断）
                    bool isTool = it.pick > 0 || it.axe > 0 || it.hammer > 0;
                    return (it.damage > 0 && !it.accessory && !isTool) || it.ammo > 0;

                case BundleCategory.Accessories:
                    return it.accessory;

                case BundleCategory.Others:
                default:
                    // 其余全部算“其他”（包括护甲、防具、药水、材料、方块等）
                    return !(it.accessory) && (it.type != ModContent.ItemType<WeaponBundleItem>()) && (it.damage <= 0 && it.ammo == 0 || it.pick > 0 || it.axe > 0 || it.hammer > 0);
            }
        }
    }
}
