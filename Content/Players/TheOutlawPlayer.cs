using Terraria;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Common.Players
{
    // 法外狂徒技能冷却，负责：暴击计数、下一次强力射击标记、后跳冷却
    public class TheOutlawPlayer : ModPlayer
    {
        // 巨鹿后：暴击后让“下一次射击”变为 6 发并 +30% 伤害
        public bool nextShotEmpowered;

        // 拜月邪教徒后：累计4次暴击，触发终极爆弹
        public int critStacksForUltimate;
        public bool ultimateReady;

        // 血肉墙后：后跳冷却（以tick计数，60tick=1秒）
        public int dashBackCooldownTicks;

        // 为避免多人联机里被不同武器误触发，这里仅统计本枪弹丸的暴击
        public void OnOurGunCrit()
        {
            // Deerclops 解锁后才生效
            if (NPC.downedDeerclops)
                nextShotEmpowered = true;

            // 拜月邪教徒解锁终极爆弹的条件
            if (NPC.downedAncientCultist)
            {
                critStacksForUltimate++;
                if (critStacksForUltimate >= 4)
                {
                    critStacksForUltimate = 0;
                    ultimateReady = true;
                }
            }

            // 血肉墙后：每次暴击缩短10秒冷却
            if (Main.hardMode)
            {
                ReduceBackstepCooldownOnCrit();
            }
        }

        public override void ResetEffects()
        {
            // 这里不重置 nextShotEmpowered/ultimateReady，它们在使用后由武器清除
        }
        public void ReduceBackstepCooldownOnCrit()
        {
            if (!Main.hardMode) return; // 只有血肉墙后有后跳
            dashBackCooldownTicks -= 120; // 2秒
            if (dashBackCooldownTicks < 0) dashBackCooldownTicks = 0;

            // 立刻刷新 Buff 显示（如果你用 Buff 展示冷却）
            int buffType = ModContent.BuffType<OutlawBackstepBuff>();
            if (dashBackCooldownTicks > 0)
            {
                Player.ClearBuff(buffType); // 先清一次，避免残留更长时间
                Player.AddBuff(buffType, dashBackCooldownTicks);
            }
            else
            {
                Player.ClearBuff(buffType);
            }
        }
        public bool CanDashBack() => Main.hardMode && dashBackCooldownTicks <= 0;

        public void TriggerDashBack()
        {
            if (!Main.hardMode) return;
            // 2分钟冷却
            dashBackCooldownTicks = 7200;
            // 实际位移与无敌帧在武器触发处完成（因为方向/速度与持武器时机更自然）
        }

        public override void PostUpdate()
        {
            if (dashBackCooldownTicks > 0)
            {
                dashBackCooldownTicks--;
                int buffType = ModContent.BuffType<OutlawBackstepBuff>();
                Player.ClearBuff(buffType);
                Player.AddBuff(buffType, dashBackCooldownTicks);
            }
        }
    }
}
