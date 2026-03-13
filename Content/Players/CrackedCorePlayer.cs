using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Players
{
    public class CrackedCorePlayer : ModPlayer
    {
        public bool coreEquipped;
        // 受伤标志位，未受伤时每秒-1，受伤则重置时间为300
        public int coreCooldown;

        public override void ResetEffects()
        {
            coreEquipped = false;
        }

        public override void PostUpdate()
        {
            if (coreCooldown > 0)
                coreCooldown--;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (coreEquipped)
            {
                coreCooldown = 300;
            }
        }
    }
}