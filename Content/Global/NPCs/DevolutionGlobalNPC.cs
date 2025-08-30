using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using WuDao.Content.Players;

namespace WuDao.Content.Global.NPCs
{
    public class DevolutionGlobalNPC : GlobalNPC
    {
        static bool AnyAuraHolderAffecting(NPC npc)
        {
            // 距离玩家的半径30格内的敌怪将受到削弱 大约15.78像素1格
            const float radius = 16f * 30;
            float r2 = radius * radius;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player plr = Main.player[i];
                if (plr == null || !plr.active || plr.dead) continue;

                if (plr.GetModPlayer<DevolutionPlayer>().HasDevolutionAura)
                {
                    // 用平方距离，别每次开方；用 Center 即可（需要更稳可改用 Hitbox.Center）
                    if (Vector2.DistanceSquared(plr.Center, npc.Center) <= r2)
                        return true;
                }
            }
            return false;
        }


        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            // 敌怪打玩家 → 伤害 ×0.6
            if (!npc.friendly && AnyAuraHolderAffecting(npc))
                modifiers.SourceDamage *= 0.6f;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (npc.friendly) return;
            if (!AnyAuraHolderAffecting(npc)) return;

            // 提高敌怪承受的伤害
            modifiers.FinalDamage *= 1.4f;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.friendly) return;
            if (!AnyAuraHolderAffecting(npc)) return;

            npc.lifeRegen = (int)(npc.lifeRegen * 0.6f);
        }
    }

}