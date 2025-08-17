using Terraria.ModLoader;

namespace WuDao.Content.Cooldowns
{
    public class GhostChakramPlayer : ModPlayer
    {
        // 0,1 -> 普通；2 -> 强化，然后回到 0
        public int GhostChakramCycle;

        public override void Initialize()
        {
            GhostChakramCycle = 0;
        }
    }
}