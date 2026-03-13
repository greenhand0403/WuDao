using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Players
{
    public class LostSymbolPlayer : ModPlayer
    {
        public int LostSymbolCount;

        public override void ResetEffects()
        {
            LostSymbolCount = 0;
        }

        public override void PostUpdateEquips()
        {
            if (LostSymbolCount > 0)
            {
                Player.GetDamage(DamageClass.Generic) += 0.01f * LostSymbolCount * LostSymbolCount;
            }
        }
    }
}