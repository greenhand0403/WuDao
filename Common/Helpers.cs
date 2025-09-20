using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace WuDao.Common
{
    public static class Helpers
    {
        public static Vector2 MouseWorld() => Main.MouseWorld;

        public static bool IsPlayerAttackingOrMoving(Player p)
        {
            // 站立或不动：不按方向键 & itemAnimation == 0
            bool moving = p.controlLeft || p.controlRight || p.controlUp || p.controlDown || p.velocity.LengthSquared() > 0.01f;
            bool attacking = p.itemAnimation > 0;
            return moving || attacking;
        }

        public static Rectangle ScreenBoundsWorldSpace()
        {
            return new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
        }
        private static UnifiedRandom rand = new UnifiedRandom();

        /// <summary>
        /// 从指定的原版 ItemID.Sets 集合中，随机获取一个满足条件的 ItemID。
        /// </summary>
        public static int GetRandomFromSet(bool[] itemSet, int fallbackItemID = ItemID.Apple)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                if (itemSet[i])
                    list.Add(i);
            }

            if (list.Count == 0)
                return fallbackItemID;

            return list[rand.Next(list.Count)];
        }

        /// <summary>
        /// 判断某个物品是否属于指定集合 ItemSetHelper.InSet(item.type, ItemID.Sets.IsFood);
        /// </summary>
        public static bool InSet(int itemID, bool[] itemSet)
        {
            return itemID >= 0 && itemID < itemSet.Length && itemSet[itemID];
        }
    }
}
