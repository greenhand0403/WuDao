using System;
using System.Collections.Generic;
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

        public override void OnEnterWorld()
        {
            // 多人客户端进服时，把本地存档中的模仿者进度同步给服务器
            if (Player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
            {
                SyncPlayer(-1, Main.myPlayer, false);
            }
        }

        public override void SaveData(TagCompound tag)
        {
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

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.SyncMimickerState);
            packet.Write((byte)Player.whoAmI);

            packet.Write(killProgress.Count);
            foreach (var kv in killProgress)
            {
                packet.Write(kv.Key);
                packet.Write(kv.Value);
            }

            packet.Write(unlockedProjectiles.Count);
            foreach (int projType in unlockedProjectiles)
                packet.Write(projType);

            packet.Send(toWho, fromWho);
        }

        public void ApplySyncedState(Dictionary<int, int> progress, HashSet<int> unlocked)
        {
            killProgress.Clear();
            foreach (var kv in progress)
                killProgress[kv.Key] = kv.Value;

            unlockedProjectiles.Clear();
            foreach (int t in unlocked)
                unlockedProjectiles.Add(t);
        }

        /// <summary>
        /// 仅服务器调用：登记一次由模仿者造成的击杀
        /// </summary>
        public void RegisterKillFromServer(int npcType)
        {
            if (!MimickerSystem.UnlockByNPC.TryGetValue(npcType, out var def))
                return;

            if (unlockedProjectiles.Contains(def.ProjectileType))
                return;

            int current = 0;
            killProgress.TryGetValue(npcType, out current);
            current++;
            killProgress[npcType] = current;

            if (current >= def.Required)
            {
                unlockedProjectiles.Add(def.ProjectileType);
                killProgress.Remove(npcType);
            }

            if (Main.netMode == NetmodeID.Server)
            {
                // 只需要同步给该玩家自己；他的 tooltip / 武器池 / 伤害都依赖这份数据
                SyncPlayer(Player.whoAmI, -1, false);
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