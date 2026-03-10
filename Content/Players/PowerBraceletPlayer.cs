using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Players
{
    public class PowerBraceletPlayer : ModPlayer
    {
        public int PowerBraceletCount;

        public override void ResetEffects()
        {
            PowerBraceletCount = 0;
        }

        public override void PostUpdateEquips()
        {
            if (PowerBraceletCount > 0)
            {
                Player.GetDamage(DamageClass.Generic) += 0.01f * PowerBraceletCount * PowerBraceletCount;
            }
        }
    }
}