using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Devlopment
{
    public class WeaponBundleItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 1;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.rare = ItemRarityID.Red;
            Item.consumable = false;
        }

        public override bool CanRightClick() => true;
        private bool IsInventoryFull(Player player)
        {
            for (int i = 0; i < Main.InventorySlotsTotal; i++)
            {
                if (player.inventory[i] == null || player.inventory[i].IsAir)
                    return false;
            }
            return true;
        }

        public override void RightClick(Player player)
        {
            int added = 0, dropped = 0;

            for (int type = 1; type < ItemLoader.ItemCount; type++)
            {
                if (type < ItemID.Count)
                {
                    Item item = new Item();
                    item.SetDefaults(type);

                    // 过滤条件：原版武器（damage>0），不是饰品，不是弹药（ammo=AmmoID.None）
                    if (item.damage > 0 && !item.accessory && item.useAmmo == AmmoID.None && !item.IsAir)
                    {
                        item.stack = 1;
                        if (IsInventoryFull(player))

                        {
                            Item.NewItem(player.GetSource_Misc("WeaponBundle"), player.Center, item.type);
                            dropped++;
                        }
                        else
                        {
                            player.QuickSpawnClonedItemDirect(player.GetSource_Misc("WeaponBundle"), item);

                            added++;
                        }
                    }
                }
            }

            Main.NewText($"添加到背包: {added} 个原版武器，掉落到地面: {dropped} 个", 255, 240, 20);
        }
    }
}
