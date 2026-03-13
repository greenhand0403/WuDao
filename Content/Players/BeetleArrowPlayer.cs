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
    }
}