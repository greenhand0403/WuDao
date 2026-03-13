using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Players
{
    public class BeetleArrowPlayer : ModPlayer
    {
        public bool beetleArrow;

        public override void ResetEffects()
        {
            beetleArrow = false;
        }
        public override void PostUpdateEquips()
        {
            if (!beetleArrow)
                return;

            // 仆从
            Player.GetCritChance(DamageClass.Summon) += 4f;

            // 鞭子
            Player.GetCritChance(DamageClass.SummonMeleeSpeed) += 4f;
        }
    }
}