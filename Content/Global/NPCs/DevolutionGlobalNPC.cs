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
        public override void PostUpdate()
        {
            // 测试用，画圈表示作用范围
            if (HasDevolutionAura && Main.netMode != NetmodeID.Server)
            {
                const float radius = 16f * 30;
                for (int k = 0; k < 12; k++)
                {
                    float ang = MathHelper.TwoPi * k / 12f;
                    Vector2 p = Player.Center + radius * ang.ToRotationVector2();
                    int d = Dust.NewDust(p - new Vector2(4), 8, 8, DustID.MagicMirror, 0, 0, 150, default, 1f);
                    Main.dust[d].noGravity = true;
                }
            }
        }
    }
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