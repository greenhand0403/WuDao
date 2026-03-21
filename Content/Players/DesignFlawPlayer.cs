using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using WuDao.Content.Global.Projectiles;

namespace WuDao.Content.Players
{
    // 败笔
    public class DesignFlawPlayer : ModPlayer
    {
        public bool hasFlaw;
        public int recordedNPCType = -1;
        public int defeatCount = 0;

        public override void ResetEffects()
        {
            hasFlaw = false;
        }
        public override void SaveData(TagCompound tag)
        {
            tag["DesignFlawRecordedNPCType"] = recordedNPCType;
            tag["DesignFlawDefeatCount"] = defeatCount;
        }

        public override void LoadData(TagCompound tag)
        {
            recordedNPCType = tag.GetInt("DesignFlawRecordedNPCType");
            defeatCount = tag.GetInt("DesignFlawDefeatCount");

            if (defeatCount <= 0)
            {
                recordedNPCType = -1;
                defeatCount = 0;
            }
        }

        public override void CopyClientState(ModPlayer targetCopy)
        {
            DesignFlawPlayer clone = (DesignFlawPlayer)targetCopy;
            clone.recordedNPCType = recordedNPCType;
            clone.defeatCount = defeatCount;
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            DesignFlawPlayer old = (DesignFlawPlayer)clientPlayer;

            if (old.recordedNPCType != recordedNPCType ||
                old.defeatCount != defeatCount)
            {
                SyncPlayer(-1, Main.myPlayer, false);
            }
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.SyncDesignFlawState);
            packet.Write((byte)Player.whoAmI);
            packet.Write(recordedNPCType);
            packet.Write(defeatCount);
            packet.Send(toWho, fromWho);
        }

        public void ClearRecord(bool sync = true)
        {
            recordedNPCType = -1;
            defeatCount = 0;

            if (sync && Main.netMode != NetmodeID.SinglePlayer)
            {
                if (Main.netMode == NetmodeID.Server || Player.whoAmI == Main.myPlayer)
                    SyncPlayer(-1, Main.netMode == NetmodeID.Server ? -1 : Main.myPlayer, false);
            }
        }
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (!hasFlaw)
                return;

            int? killerBossNpcIndex = null;

            // 1) 直接击杀者就是 NPC
            if (damageSource.SourceNPCIndex >= 0)
            {
                NPC n = Main.npc[damageSource.SourceNPCIndex];
                if (n.active && (n.boss || NPCID.Sets.ShouldBeCountedAsBoss[n.type]))
                    killerBossNpcIndex = n.whoAmI;
            }

            // 2) 被弹幕击杀 → 通过 GlobalProjectile 找到“发射它的 NPC”
            if (killerBossNpcIndex == null && damageSource.SourceProjectileLocalIndex >= 0)
            {
                Projectile proj = Main.projectile[damageSource.SourceProjectileLocalIndex];
                if (proj.active)
                {
                    var gp = proj.GetGlobalProjectile<DesignFlawGlobalProjectile>();
                    int ownerNpc = gp?.OwnerNPC ?? -1;
                    if (ownerNpc >= 0)
                    {
                        NPC n = Main.npc[ownerNpc];
                        if (n.active && (n.boss || NPCID.Sets.ShouldBeCountedAsBoss[n.type]))
                            killerBossNpcIndex = n.whoAmI;
                    }
                }
            }

            // 3) 仍没找到 → 选择“距离玩家最近的 Boss”
            // if (killerBossNpcIndex == null)
            // {
            //     float best = float.MaxValue;
            //     int bestIdx = -1;
            //     for (int i = 0; i < Main.maxNPCs; i++)
            //     {
            //         NPC n = Main.npc[i];
            //         if (!n.active) continue;
            //         if (!(n.boss || NPCID.Sets.ShouldBeCountedAsBoss[n.type])) continue;

            //         float d = Vector2.Distance(Player.Center, n.Center);
            //         if (d < best)
            //         {
            //             best = d;
            //             bestIdx = i;
            //         }
            //     }
            //     if (bestIdx >= 0)
            //         killerBossNpcIndex = bestIdx;
            // }

            // 4) 只记录 Boss：有就记录/覆盖；没有就不动
            if (killerBossNpcIndex is int idx && idx >= 0)
            {
                NPC boss = Main.npc[idx];
                // 覆盖逻辑：未记录、或记录的不是这个 Boss → 重置并从 1 开始
                if (recordedNPCType != boss.type)
                {
                    recordedNPCType = boss.type;
                    defeatCount = 1;
                }
                else
                {
                    defeatCount++;
                }
                // 多人模式时同步记录
                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    if (Main.netMode == NetmodeID.Server || Player.whoAmI == Main.myPlayer)
                        SyncPlayer(-1, Main.netMode == NetmodeID.Server ? -1 : Main.myPlayer, false);
                }
            }
            // 若仍为 null → 无法确定 Boss，保持原记录不变
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (hasFlaw && target.type == recordedNPCType && defeatCount > 0)
            {
                modifiers.FinalDamage *= 1f + (0.1f * defeatCount);
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (hasFlaw && target.type == recordedNPCType && defeatCount > 0)
            {
                modifiers.FinalDamage *= 1f + (0.1f * defeatCount);
            }
        }
    }

}
