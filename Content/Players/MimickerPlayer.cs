using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using WuDao.Content.Systems;

namespace WuDao.Content.Players
{
    // 模仿者 射弹收集系统
    public class MimickerPlayer : ModPlayer
    {
        // 进度：针对每个定义，记录击杀计数
        public Dictionary<int, int> killProgress = new(); // key = NPCID, value = kills with Mimicker
        // 已解锁的弹体集合
        public HashSet<int> unlockedProjectiles = new();

        public override void Initialize()
        {
            killProgress.Clear();
            unlockedProjectiles.Clear();
        }

        public override void SaveData(TagCompound tag)
        {
            // 存储解锁与进度
            var progressPairs = new List<int>();
            foreach (var kv in killProgress)
            {
                progressPairs.Add(kv.Key);
                progressPairs.Add(kv.Value);
            }
            tag["MimickerKills"] = progressPairs;
            tag["MimickerUnlocked"] = new List<int>(unlockedProjectiles);
        }

        public override void LoadData(TagCompound tag)
        {
            killProgress.Clear();
            unlockedProjectiles.Clear();
            if (tag.TryGet("MimickerKills", out List<int> progressPairs))
            {
                for (int i = 0; i + 1 < progressPairs.Count; i += 2)
                    killProgress[progressPairs[i]] = progressPairs[i + 1];
            }
            if (tag.TryGet("MimickerUnlocked", out List<int> unlocked))
            {
                foreach (var t in unlocked)
                    unlockedProjectiles.Add(t);
            }
        }

        public int TotalTypesUnlocked(out int baseCount)
        {
            baseCount = MimickerSystem.BasePool.Length;
            return baseCount + unlockedProjectiles.Count;
        }

        public IEnumerable<int> BuildCurrentPool()
        {
            int unlockedCount = unlockedProjectiles.Count;
            int baseLen = MimickerSystem.BasePool.Length;
            int remainBase = Math.Max(0, baseLen - unlockedCount);

            // 只保留基础池的前 remainBase 个（其余被“替换/挤出”）
            for (int i = 0; i < remainBase; i++)
                yield return MimickerSystem.BasePool[i];

            // 已解锁的全部加入
            foreach (var t in unlockedProjectiles)
                yield return t;
        }

    }
}