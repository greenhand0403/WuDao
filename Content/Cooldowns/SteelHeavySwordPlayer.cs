using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Cooldowns
{
    // 玄铁重剑的冷却时间
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
