using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Global.NPCs
{
    // 压制力场：范围内每个装备者提供1层效果
    public class DevolutionGlobalNPC : GlobalNPC
    {
        private const int MaxStacks = Main.maxPlayers;
        private const float DamageToPlayerPerStack = 0.9f; // 敌怪打玩家每层×0.9
        private const float DamageToNpcPerStack = 1.1f;    // 敌怪承伤每层×1.1

        private static int GetAuraStackCount(Vector2 targetCenter)
        {
            float r2 = DevolutionPlayer.AuraRadius * DevolutionPlayer.AuraRadius;
            int stacks = 0;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player plr = Main.player[i];
                if (plr == null || !plr.active || plr.dead)
                    continue;

                if (!plr.GetModPlayer<DevolutionPlayer>().HasDevolutionAura)
                    continue;

                if (Vector2.DistanceSquared(plr.Center, targetCenter) <= r2)
                {
                    stacks++;
                    if (stacks >= MaxStacks)
                        return MaxStacks;
                }
            }

            return stacks;
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (npc.friendly)
                return;

            int stacks = GetAuraStackCount(npc.Center);
            if (stacks <= 0)
                return;

            float mult = 1f;
            for (int i = 0; i < stacks; i++)
                mult *= DamageToPlayerPerStack;

            modifiers.SourceDamage *= mult;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (npc.friendly)
                return;

            int stacks = GetAuraStackCount(npc.Center);
            if (stacks <= 0)
                return;

            float mult = 1f;
            for (int i = 0; i < stacks; i++)
                mult *= DamageToNpcPerStack;

            modifiers.FinalDamage *= mult;
        }
    }
}