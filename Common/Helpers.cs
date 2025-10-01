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
            private const int MaxCountForLinear = 17;     // 线性增长封顶的计数，目前统计BOSS数量最大17
            private const float DamagePerBoss = 0.04f;    // 每个BOSS +4% 伤害
            private const float ProjSpeedPerBoss = 0.03f; // 每个BOSS +3% 射弹速度
            private const float MeleeScalePerBoss = 0.02f; // 每个BOSS +2% 近战尺寸
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

        // 第一滴血判断已击败BOSS的辅助方法
        // 判断是否已击败所有原版 Boss（可按你需求增减）
        public static bool AllVanillaBossesDowned()
        {
            return NPC.downedSlimeKing
                && NPC.downedBoss1      // 眼
                && NPC.downedBoss2      // 世/脑
                && NPC.downedBoss3      // 地牢骷髅
                && NPC.downedQueenBee
                && NPC.downedDeerclops
                && Main.hardMode        // 肉山
                && NPC.downedMechBossAny  // 机械三王至少一个，若想要求三王全清可改成：downedMechBoss1 && downedMechBoss2 && downedMechBoss3
                && NPC.downedPlantBoss
                && NPC.downedGolemBoss
                && NPC.downedFishron
                && NPC.downedQueenSlime
                && NPC.downedAncientCultist
                && NPC.downedMoonlord;
        }
        public static List<string> GetRemainingVanillaBossNames()
        {
            var list = new List<string>();

            // 这里与你效果判定同一套“原版BOSS集合”，并用 downed 标志判断
            // 你可以与 IsBossDefeated/AllVanillaBossesDowned 的逻辑保持一致
            void AddIfNotDowned(int npcType, bool defeatedFlag)
            {
                if (!defeatedFlag)
                    list.Add(Lang.GetNPCNameValue(npcType));
            }

            AddIfNotDowned(NPCID.KingSlime, NPC.downedSlimeKing);
            AddIfNotDowned(NPCID.EyeofCthulhu, NPC.downedBoss1);
            // 世/脑两选一算通过，这里仅在都没打时显示两者
            if (!NPC.downedBoss2)
            {
                list.Add(Lang.GetNPCNameValue(NPCID.EaterofWorldsHead));
                list.Add(Lang.GetNPCNameValue(NPCID.BrainofCthulhu));
            }
            AddIfNotDowned(NPCID.SkeletronHead, NPC.downedBoss3);
            AddIfNotDowned(NPCID.QueenBee, NPC.downedQueenBee);
            AddIfNotDowned(NPCID.Deerclops, NPC.downedDeerclops);
            AddIfNotDowned(NPCID.WallofFlesh, Main.hardMode);

            // 机械三王：如果 downedMechBossAny 但不是“三王全清”，显示具体未击败者
            bool mechAny = NPC.downedMechBossAny;
            bool mech1 = NPC.downedMechBoss1; // 双子魔眼
            bool mech2 = NPC.downedMechBoss2; // 骷髅王Prime
            bool mech3 = NPC.downedMechBoss3; // 毁灭者
            if (!(mech1 && mech2 && mech3))
            {
                if (!mech1)
                {
                    list.Add(Lang.GetNPCNameValue(NPCID.Retinazer));
                    list.Add(Lang.GetNPCNameValue(NPCID.Spazmatism));
                }
                if (!mech2) list.Add(Lang.GetNPCNameValue(NPCID.SkeletronPrime));
                if (!mech3) list.Add(Lang.GetNPCNameValue(NPCID.TheDestroyer));
            }

            AddIfNotDowned(NPCID.Plantera, NPC.downedPlantBoss);
            AddIfNotDowned(NPCID.Golem, NPC.downedGolemBoss);
            AddIfNotDowned(NPCID.DukeFishron, NPC.downedFishron);
            AddIfNotDowned(NPCID.QueenSlimeBoss, NPC.downedQueenSlime);
            AddIfNotDowned(NPCID.CultistBoss, NPC.downedAncientCultist);
            AddIfNotDowned(NPCID.MoonLordCore, NPC.downedMoonlord);

            return list;
        }
    }
}
