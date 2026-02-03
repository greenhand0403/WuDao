// ======================== WeaponBundleItem.cs ========================
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace WuDao.Content.Development
{
    // 开局礼包物品
    public enum BundleCategory
    {
        Weapons,
        Accessories,
        Others
    }

    public class WeaponBundleItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 可选：显示“右键使用”提示
            // Tooltip.SetDefault("右键打开选择面板：武器 / 饰品 / 其他");
        }

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
            // 打开我们的 UI（把本次触发玩家与回调传给系统）
            BundleSelectSystem.Show(category => GiveItemsForCategory(player, category, Mod));
            Main.playerInventory = true; // 打开背包，避免 UI 被遮挡/鼠标锁定
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        private void GiveItemsForCategory(Player player, BundleCategory category, Mod mod)
        {
            int addedKinds = 0;
            int droppedKinds = 0;
            int droppedTotal = 0;
            var src = player.GetSource_Misc("WuDaoBundle");

            for (int type = 1; type < ItemLoader.ItemCount; type++)
            {
                Item probe = new Item();
                probe.SetDefaults(type);

                if (probe.IsAir) continue;
                if (probe.ModItem?.Mod != mod) continue; // 只给 WuDao 模组物品
                if (type == Type) continue;              // 排除礼包本体

                // 分类筛选
                if (!BelongsToCategory(probe, category))
                    continue;

                int stack = probe.maxStack > 1 ? probe.maxStack : 1;

                Item give = new Item();
                give.SetDefaults(type);
                give.stack = stack;

                // 使用 GetItem 让引擎尽可能塞入背包，返回“剩余”
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

            Main.NewText(
                $"领取完成：进入背包 {addedKinds} 种；掉落 {droppedKinds} 种（共 {droppedTotal} 件/叠）。",
                255, 240, 20
            );

            // 领取完毕后自动关闭 UI
            BundleSelectSystem.Hide();

            // TODO（多人联机同步）：
            // 若此礼包可在客户端使用，需要用 ModPacket 广播所选类别并在服务器执行 GiveItemsForCategory，
            // 或者仅允许在服务器端/单机端调用（检查 Main.netMode）。
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
