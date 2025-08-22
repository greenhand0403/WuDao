using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using WuDao.Content.Items.Accessories;

namespace WuDao.Content.Global.NPCs
{
    public class DevolutionPlayer : ModPlayer
    {
        public bool HasDevolutionAura;

        public override void ResetEffects()
        {
            HasDevolutionAura = false; // 每帧重置
        }
    }
    public class DevolutionGlobalNPC : GlobalNPC
    {
        static bool AnyAuraHolderAffecting(NPC npc)
        {
            // 距离玩家的半径r的敌怪将受到削弱 屏幕半径（近似）：用对角线一半，或你自定常量例如 900f
            // float radius = (float)Math.Sqrt(Main.screenWidth * Main.screenWidth + Main.screenHeight * Main.screenHeight) * 0.5f;
            float radius = 10f;// 仅供测试
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player plr = Main.player[i];
                if (plr == null || !plr.active) continue;

                if (plr.GetModPlayer<DevolutionPlayer>().HasDevolutionAura)
                {
                    if (Vector2.Distance(plr.Center, npc.Center) <= radius)
                        return true;
                }
            }
            return false;
        }


        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            // 敌怪打玩家 → 伤害 ×0.8
            if (!npc.friendly && AnyAuraHolderAffecting(npc))
                modifiers.SourceDamage *= 0.8f;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (npc.friendly) return;
            if (!AnyAuraHolderAffecting(npc)) return;

            // 提高敌怪承受的伤害 25% 
            modifiers.FinalDamage *= 1.25f;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.friendly) return;
            if (!AnyAuraHolderAffecting(npc)) return;

            npc.lifeRegen = (int)(npc.lifeRegen * 0.8f);
        }
    }

}