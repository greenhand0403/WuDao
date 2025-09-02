using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using Terraria.Localization;

namespace WuDao.Content.Players
{
    public class ApeTouchPlayer : ModPlayer
    {
        public bool ApeTouch;
        // 5秒冷却（300 tick）
        private const int ManaGuardCDTicks = 60 * 5;
        private int manaGuardCooldown;

        // 防止同一帧/同一次伤害链多次扣蓝
        private bool manaGuardThisHit;

        public override void UpdateDead()
        {
            manaGuardCooldown = 0;
            manaGuardThisHit = false;
        }

        public override void PreUpdate()
        {
            if (manaGuardCooldown > 0) manaGuardCooldown--;
            // 每帧清掉一次性标记
            manaGuardThisHit = false;
        }

        // —— 只对 NPC 接触伤害生效的“法力低消” —— //
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (!ApeTouch) return;
            if (Player.statLife > Player.statLifeMax2 * 0.51f) return;
            if (manaGuardCooldown > 0) return;
            if (npc == null || !npc.active || npc.friendly) return;

            // 在最终伤害生成时介入（这一步仍处于一次受击的计算链内，还没真正扣血）
            modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) =>
            {
                // 同一条伤害链只处理一次
                if (manaGuardThisHit) return;

                // 此时 info.Damage 就是“将要扣到血条上”的最终整数伤害
                int finalIncoming = info.Damage;
                if (finalIncoming <= 0) return;

                // 规则：用“两倍法力”抵消 50% 伤害 ⇒ 需要法力 = 2 * (50% 的伤害) = finalIncoming
                int manaNeeded = finalIncoming;

                if (Player.statMana >= manaNeeded)
                {
                    // 先扣法力（仍处于结算链内，尚未扣血）
                    Player.statMana -= manaNeeded;
                    Player.ManaEffect(manaNeeded);

                    // 再把即将结算到生命值的伤害改成一半（向上取整避免为0）
                    info.Damage = Math.Max(1, (finalIncoming + 1) / 2);

                    // 进入 5 秒冷却
                    manaGuardCooldown = ManaGuardCDTicks;
                    manaGuardThisHit = true;
                }
                // 法力不够：不改 info.Damage，本次不生效，也不进CD
            };
        }
    }
}