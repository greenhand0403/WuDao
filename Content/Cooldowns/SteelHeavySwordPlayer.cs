using Terraria;
using Terraria.Audio;
using Terraria.ID;
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
            {
                RightClickCooldown--;
                if (RightClickCooldown == 0)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana, Player.position);
                }
            }
        }
    }
}
