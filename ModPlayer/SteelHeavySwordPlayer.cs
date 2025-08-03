using Terraria;
using Terraria.ModLoader;

namespace WuDao
{
    public class SteelHeavySwordPlayer : ModPlayer
    {
        public int RightClickCooldown;

        public override void ResetEffects()
        {
            if (RightClickCooldown > 0)
                RightClickCooldown--;
        }
    }
}
