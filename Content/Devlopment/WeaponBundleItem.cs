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

        public override void RightClick(Player player)
        {
            int addedKinds = 0;            // 完整进入背包的“物品种类”数
            int droppedKinds = 0;          // 有任意剩余被丢在地上的“物品种类”数
            int droppedTotalStacks = 0;    // 实际丢在地上的总数量（件/叠）

            var src = player.GetSource_Misc("WuDaoBundle");

            for (int type = 1; type < ItemLoader.ItemCount; type++)
            {
                // 只处理本模组的物品，且排除礼包本体
                Item probe = new Item();
                probe.SetDefaults(type);
                if (probe.IsAir || probe.ModItem?.Mod != Mod || type == Type)
                    continue;

                int stack = probe.maxStack > 1 ? probe.maxStack : 1;

                // 构造要给予的物品
                Item give = new Item();
                give.SetDefaults(type);
                give.stack = stack;

                // 尝试放入玩家背包，返回“剩余拿不下”的部分
                Item leftover = player.GetItem(player.whoAmI, give, GetItemSettings.LootAllSettings);

                if (leftover != null && leftover.stack > 0)
                {
                    // 有剩余：把剩余那部分丢在地上
                    Item.NewItem(src, player.getRect(), leftover.type, leftover.stack);
                    droppedKinds++;
                    droppedTotalStacks += leftover.stack;
                }
                else
                {
                    // 全部成功进入背包
                    addedKinds++;
                }
            }

            Main.NewText(
                $"WuDao礼包结算：进入背包 {addedKinds} 种；掉落地面 {droppedKinds} 种（共 {droppedTotalStacks} 件/叠）",
                255, 240, 20
            );
        }
    }
}
