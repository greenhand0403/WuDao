using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using WuDao.Content.Items.Accessories;
using WuDao.Common;

namespace WuDao.Content.Players
{
    // 第一滴血
    public class FirstBloodPlayer : ModPlayer
    {
        public bool hasFirstBlood;

        public override void ResetEffects()
        {
            hasFirstBlood = false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            ApplyBonus(target, ref modifiers);
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            ApplyBonus(target, ref modifiers);
        }

        private void ApplyBonus(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasFirstBlood) return;

            if (Helpers.AllVanillaBossesDowned())
            {
                // 勇者之证：对 >90% 生命值的敌怪“首次攻击”+300%
                // “首次攻击”可用本地标记实现，这里示例为：目标满血时触发
                if (target.life >= (int)(target.lifeMax * 0.9f) && target.life == target.lifeMax)
                    modifiers.FinalDamage *= 4f; // +300%
            }
            else
            {
                // 第一滴血：对尚未击败的原版 Boss +10%
                if (IsTrackedVanillaBoss(target.type) && !IsBossDefeated(target.type))
                    modifiers.FinalDamage *= 1.1f;
            }
        }

        private static bool IsTrackedVanillaBoss(int type)
        {
            return type == NPCID.KingSlime
                || type == NPCID.EyeofCthulhu
                || type == NPCID.EaterofWorldsHead || type == NPCID.BrainofCthulhu
                || type == NPCID.QueenBee
                || type == NPCID.SkeletronHead
                || type == NPCID.Deerclops
                || type == NPCID.WallofFlesh
                || type == NPCID.TheDestroyer || type == NPCID.SkeletronPrime || type == NPCID.Retinazer || type == NPCID.Spazmatism
                || type == NPCID.Plantera
                || type == NPCID.Golem
                || type == NPCID.DukeFishron
                || type == NPCID.QueenSlimeBoss
                || type == NPCID.CultistBoss
                || type == NPCID.MoonLordCore;
        }

        private static bool IsBossDefeated(int type)
        {
            return type switch
            {
                NPCID.KingSlime => NPC.downedSlimeKing,
                NPCID.EyeofCthulhu => NPC.downedBoss1,
                NPCID.EaterofWorldsHead => NPC.downedBoss2,
                NPCID.BrainofCthulhu => NPC.downedBoss2,
                NPCID.QueenBee => NPC.downedQueenBee,
                NPCID.SkeletronHead => NPC.downedBoss3,
                NPCID.Deerclops => NPC.downedDeerclops,
                NPCID.WallofFlesh => Main.hardMode,
                NPCID.TheDestroyer => NPC.downedMechBoss3 || NPC.downedMechBossAny,
                NPCID.SkeletronPrime => NPC.downedMechBoss2 || NPC.downedMechBossAny,
                NPCID.Retinazer => NPC.downedMechBoss1 || NPC.downedMechBossAny,
                NPCID.Spazmatism => NPC.downedMechBoss1 || NPC.downedMechBossAny,
                NPCID.Plantera => NPC.downedPlantBoss,
                NPCID.Golem => NPC.downedGolemBoss,
                NPCID.DukeFishron => NPC.downedFishron,
                NPCID.QueenSlimeBoss => NPC.downedQueenSlime,
                NPCID.CultistBoss => NPC.downedAncientCultist,
                NPCID.MoonLordCore => NPC.downedMoonlord,
                _ => false
            };
        }
    }
}