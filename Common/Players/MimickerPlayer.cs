using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace WuDao.Common.Players
{
    // 模仿者 特殊魔法武器的辅助类 解锁定义：指定NPC -> 解锁的友方射弹 -> 所需击杀数
    public struct UnlockDef
    {
        public int NpcType;
        public int ProjectileType;
        public int Required;
        public string DisplayName; // 用于提示

        public UnlockDef(int npcType, int projType, int required, string display)
        {
            NpcType = npcType;
            ProjectileType = projType;
            Required = required;
            DisplayName = display;
        }
    }

    public class MimickerSystem : ModSystem
    {
        // 你可以根据需要增删，尽量选择“发射弹幕的远程/施法型敌怪”
        public static readonly UnlockDef[] UnlockTable = new UnlockDef[]
        {
            // 例：击败 Ichor Sticker(灵液黏黏怪) 解锁 黄金雨 友方弹
            new UnlockDef(NPCID.IchorSticker, ProjectileID.GoldenShowerFriendly, 1, "黄金雨"),
            // 腐化者（喷吐咒火弹） -> 诅咒焰
            new UnlockDef(NPCID.Corruptor, ProjectileID.CursedFlameFriendly, 1, "诅咒焰"),
            // 地牢死灵施法者 -> 暗影光束
            new UnlockDef(NPCID.Necromancer, ProjectileID.ShadowBeamFriendly, 1, "暗影光束"),
            // 地牢幻魂 -> 幻魂弹（幽灵怨魂）
            new UnlockDef(NPCID.DungeonSpirit, ProjectileID.SpectreWrath, 1, "幽魂之怒"),
            // 地下寒霜法师 -> 水晶风暴
            new UnlockDef(NPCID.IceElemental, ProjectileID.CrystalStorm, 1, "水晶风暴"),
        };

        public static readonly int[] BasePool = new int[]
        {
            ProjectileID.AmethystBolt,
            ProjectileID.TopazBolt,
            ProjectileID.SapphireBolt,
            ProjectileID.EmeraldBolt,
            ProjectileID.RubyBolt,
            ProjectileID.DiamondBolt,
        };
    }

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

    // 用于标记“是否来自模仿者”的弹体
    public class MimickerGlobalProjectile : GlobalProjectile
    {
        public bool fromMimicker;
        public override bool InstancePerEntity => true;
    }

    // 记录“最后一次受到来自模仿者弹体的伤害”的NPC
    public class MimickerGlobalNPC : GlobalNPC
    {
        public bool lastHitByMimicker;
        public int lastHitterPlayer = -1;
        public override bool InstancePerEntity => true;
        public int mimickerHitTimer; // 以tick计时，>0 表示最近被模仿者击中过

        // public override void ResetEffects(NPC npc)
        // {
        //     lastHitByMimicker = false; // 每tick重置，由 OnHitByProjectile 再标记
        //     lastHitterPlayer = -1;
        // }
        public override void AI(NPC npc)
        {
            if (mimickerHitTimer > 0) mimickerHitTimer--;
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.GetGlobalProjectile<MimickerGlobalProjectile>().fromMimicker)
            {
                mimickerHitTimer = 60; // 最近1秒内算模仿者命中过
                lastHitterPlayer = projectile.owner; // 由子弹的 owner 获取玩家 whoAmI
            }
        }

        public override void OnKill(NPC npc)
        {
            if (mimickerHitTimer <= 0 || lastHitterPlayer < 0)
                return;

            Player p = Main.player[lastHitterPlayer];
            if (p == null || !p.active) return;

            var mp = p.GetModPlayer<MimickerPlayer>();
            foreach (var def in MimickerSystem.UnlockTable)
            {
                if (def.NpcType == npc.type)
                {
                    mp.killProgress.TryGetValue(def.NpcType, out int cur);
                    cur++;
                    mp.killProgress[def.NpcType] = cur;

                    if (cur == def.Required)
                    {
                        mp.unlockedProjectiles.Add(def.ProjectileType);
                        if (p.whoAmI == Main.myPlayer)
                        {
                            CombatText.NewText(p.getRect(), new Color(255, 220, 100), $"解锁：{def.DisplayName} 射弹！");
                            Main.NewText($"[模仿者] 你已解锁 {def.DisplayName} 射弹。", 255, 240, 150);
                        }
                    }
                    else if (p.whoAmI == Main.myPlayer && cur < def.Required)
                    {
                        int remain = def.Required - cur;
                        CombatText.NewText(p.getRect(), new Color(180, 220, 255), $"{def.DisplayName} 解锁还需 {remain}");
                    }
                }
            }
        }

    }
}