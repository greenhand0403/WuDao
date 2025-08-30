using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Players
{
    public class HeartStuffPlayer : ModPlayer
    {
        public bool SoulGemEquipped;

        public override void ResetEffects()
        {
            SoulGemEquipped = false;
        }

        /// <summary>根据当前状态返回本次开火应扣的生命：默认2；丘比特弓=1；心灵宝石=0。</summary>
        public int GetHeartArrowLifeCost(bool usingCupidBow)
        {
            if (SoulGemEquipped) return 0;
            return usingCupidBow ? 1 : 2;
        }
    }
}

