using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Global.Projectiles;
using WuDao.Content.Items.Accessories;

namespace WuDao.Content.Players
{
    public class DesignFlawPlayer : ModPlayer
    {
        public bool hasFlaw;
        public int recordedNPCType = -1;
        public int defeatCount = 0;

        public override void ResetEffects()
        {
            hasFlaw = false;
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
                    var gp = proj.GetGlobalProjectile<BossOwnerGlobalProjectile>();
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
            if (killerBossNpcIndex == null)
            {
                float best = float.MaxValue;
                int bestIdx = -1;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (!n.active) continue;
                    if (!(n.boss || NPCID.Sets.ShouldBeCountedAsBoss[n.type])) continue;

                    float d = Vector2.Distance(Player.Center, n.Center);
                    if (d < best)
                    {
                        best = d;
                        bestIdx = i;
                    }
                }
                if (bestIdx >= 0)
                    killerBossNpcIndex = bestIdx;
            }

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
