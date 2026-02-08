using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items.Weapons.Summon;

namespace WuDao.Content.Systems
{
    // 世界生成完毕后，往指定宝箱里塞物品的标准做法
    public class LivingTreeChestLootSystem : ModSystem
    {
        public override void PostWorldGen()
        {
            int bugStuffType = ModContent.ItemType<BugStuff>();

            for (int i = 0; i < Main.maxChests; i++)
            {
                Chest chest = Main.chest[i];
                if (chest == null)
                    continue;

                // 识别“生命树宝箱”
                if (!IsLivingTreeChest(chest))
                    continue;

                // 50% 概率塞入
                if (Main.rand.NextFloat() < 0.50f)
                {
                    TryPlaceItemInChest(chest, bugStuffType);
                }
            }
        }

        /// <summary>
        /// 用“生命树宝箱常见专属物品”来判定（比用 frameX/style 更稳）
        /// </summary>
        private static bool IsLivingTreeChest(Chest chest)
        {
            // 生命树宝箱最具代表性的内容：生命木魔杖 / 树叶魔杖 / 生命织机
            //（就算不是每次都有，但出现概率高，做判定很实用）
            for (int slot = 0; slot < Chest.maxItems; slot++)
            {
                int type = chest.item[slot]?.type ?? 0;
                if (type == ItemID.LivingWoodWand || type == ItemID.LeafWand || type == ItemID.LivingLoom)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 优先找空位放入；若满了就不放（避免覆盖原版战利品）
        /// </summary>
        private static void TryPlaceItemInChest(Chest chest, int itemType, int stack = 1)
        {
            // 先找空位
            for (int slot = 0; slot < Chest.maxItems; slot++)
            {
                if (chest.item[slot].IsAir)
                {
                    chest.item[slot].SetDefaults(itemType);
                    chest.item[slot].stack = stack;
                    return;
                }
            }

            // 如果你希望“满了也要塞进去”，可以改成替换一个普通材料位（不推荐默认这么干）
        }
    }
}
