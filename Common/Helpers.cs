using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using WuDao.Content.Systems;

namespace WuDao.Common
{
    public static class Helpers
    {
        // 绘制有关的辅助方法
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
        /// <summary>按已击败唯一BOSS数量提供的加成数值。</summary>
        public readonly struct BossProgressBonus
        {
            public readonly int BossCount;          // 已击败唯一BOSS数
            public readonly float DamageMult;       // 伤害乘区
            public readonly float ProjSpeedMult;    // 射弹速度乘区
            public readonly float MeleeScaleMult;   // 近战武器尺寸（Item.scale）乘区
            public readonly int MeleeRangePixels;   // 近战判定范围（像素）附加量（可用于自定义近战判定）

            public BossProgressBonus(int count, float dmg, float ps, float ms, int mr)
            {
                BossCount = count;
                DamageMult = dmg;
                ProjSpeedMult = ps;
                MeleeScaleMult = ms;
                MeleeRangePixels = mr;
            }
        }

        public static class BossProgressPower
        {
            // —— 可调参数（给你默认一套克制的成长线；想改请调这里）——
            private const int MaxCountForLinear = 15;     // 线性增长封顶的计数
            private const float DamagePerBoss = 0.01f;    // 每个BOSS +1% 伤害，上限 ~15%
            private const float ProjSpeedPerBoss = 0.01f; // 每个BOSS +1% 射弹速度
            private const float MeleeScalePerBoss = 0.005f; // 每个BOSS +0.5% 近战尺寸
            private const int MeleeRangePerBoss = 3;      // 每个BOSS +3px 近战额外判定范围

            /// <summary>获取当前玩家的“按BOSS进度成长”的一组加成。</summary>
            public static BossProgressBonus Get(Player player)
            {
                int count = GetUniqueBossCount();

                int eff = count > MaxCountForLinear ? MaxCountForLinear : count;
                float dmg = 1f + eff * DamagePerBoss;
                float ps = 1f + eff * ProjSpeedPerBoss;
                float ms = 1f + eff * MeleeScalePerBoss;
                int mr = eff * MeleeRangePerBoss;

                return new BossProgressBonus(count, dmg, ps, ms, mr);
            }

            /// <summary>唯一BOSS数量（同组只记一次）。</summary>
            public static int GetUniqueBossCount()
            {
                return BossDownedSystem.DownedBossGroups.Count;
            }
        }
    }
}
