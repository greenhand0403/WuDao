using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace WuDao.Content.Systems
{
    // 境界机制：随着计算的BOSS种类增加，部分物品的伤害、近战范围、射弹速度会增加
    // 1) 世界级别保存：记录“已首杀的BOSS组”集合（去重）
    class BossDownedSystem : ModSystem
    {
        // 用“Boss组ID”（见BossGroupHelper）做去重键
        public static readonly HashSet<string> DownedBossGroups = new();

        public override void OnWorldLoad() => DownedBossGroups.Clear();
        public override void OnWorldUnload() => DownedBossGroups.Clear();

        public override void SaveWorldData(TagCompound tag)
        {
            tag["downedBossGroups"] = new List<string>(DownedBossGroups);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            DownedBossGroups.Clear();
            if (tag.ContainsKey("downedBossGroups"))
            {
                foreach (var g in tag.GetList<string>("downedBossGroups"))
                    DownedBossGroups.Add(g);
            }
        }
    }
    // 2) 首杀钩子：当任意BOSS死亡时，把其映射成“Boss组ID”，仅首次加入集合
    public class BossKillTracker : GlobalNPC
    {
        public override bool InstancePerEntity => false;

        public override void OnKill(NPC npc)
        {
            if (!npc.boss || npc.friendly || npc.type <= NPCID.None)
                return;

            string group = BossGroupHelper.GetGroupIdFor(npc.type);
            if (!string.IsNullOrEmpty(group))
                BossDownedSystem.DownedBossGroups.Add(group);
        }
    }
    // 3) 把“同名/同组BOSS”合并为一个组ID（避免 Twins 计两次等）
    internal static class BossGroupHelper
    {
        // 你可按需扩展：键=NPCID；值=组ID（同组共用同一个字符串）
        // 组ID只要保持一致即可，这里用英文短名，方便世界保存。
        private static readonly Dictionary<int, string> Map = new()
        {
            // 史莱姆王
            [NPCID.KingSlime] = "KingSlime",
            // 眼球
            [NPCID.EyeofCthulhu] = "Eye",
            // 世吞 / 克脑
            [NPCID.EaterofWorldsHead] = "EaterOfWorlds",  // 只统计头
            [NPCID.BrainofCthulhu] = "Brain",
            // 蜂后
            [NPCID.QueenBee] = "QueenBee",
            // 肉山
            [NPCID.WallofFlesh] = "WoF",
            // 鹿角怪
            [NPCID.Deerclops] = "Deerclops",

            // 机械三王：各算一次（不合并）
            [NPCID.TheDestroyer] = "Destroyer",
            [NPCID.SkeletronPrime] = "Prime",
            [NPCID.Retinazer] = "Twins",     // Twins 只算一次
            [NPCID.Spazmatism] = "Twins",

            // 世纪之花、石巨人、公爵、女皇、邪教徒、月总
            [NPCID.Plantera] = "Plantera",
            [NPCID.Golem] = "Golem",
            [NPCID.DukeFishron] = "Fishron",
            [NPCID.HallowBoss] = "Empress",
            [NPCID.CultistBoss] = "Cultist",
            [NPCID.MoonLordCore] = "MoonLord",

            // 女王史莱姆
            [NPCID.QueenSlimeBoss] = "QueenSlime",
        };

        public static string GetGroupIdFor(int npcType)
        {
            if (Map.TryGetValue(npcType, out var group))
                return group;

            // 兜底：若未知BOSS，退化为其 type 字符串，至少能去重“同一个类型”
            // 这样其它模组BOSS也能被记住（如果它们有 npc.boss=true）
            return $"Boss_{npcType}";
        }
    }
}