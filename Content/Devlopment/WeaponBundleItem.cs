using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Devlopment
{
    public class WeaponBundleItem : ModItem
    {
        // 现在：右键获得 WuDao 模组的全部物品；可堆叠的按最大堆叠给予
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

            // 遍历所有加载的物品类型
            for (int type = 1; type < ItemLoader.ItemCount; type++)
            {
                // 用临时 Item 来读取该 type 的信息
                Item temp = new Item();
                temp.SetDefaults(type);

                // 只发放本模组(WuDao)的物品，且排除礼包本体，排除无效物品
                // 注意：在 ModItem 类里，this.Mod 指向当前模组
                if (temp == null || temp.IsAir)
                    continue;

                var mi = temp.ModItem;
                if (mi == null || mi.Mod != Mod)
                    continue;

                if (type == Type) // 不把礼包本体也送给玩家，避免循环刷包
                    continue;

                // 设置堆叠数：可堆叠就满堆叠，否则为 1
                int stack = temp.maxStack > 1 ? temp.maxStack : 1;

                // 构造将要给予的实例
                Item give = new Item();
                give.SetDefaults(type);
                give.stack = stack;

                if (IsInventoryFull(player))
                {
                    // 背包已满：掉落在脚下
                    Item.NewItem(player.GetSource_Misc("WuDaoBundle"), player.Center, give.type, give.stack);
                    dropped++;
                }
                else
                {
                    // 背包未满：直接放入背包
                    player.QuickSpawnClonedItemDirect(player.GetSource_Misc("WuDaoBundle"), give);
                    added++;
                }
            }

            Main.NewText($"添加到背包: {added} 件WuDao物品，掉落到地面: {dropped} 件", 255, 240, 20);
        }
    }
}
