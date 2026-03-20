using Terraria.ModLoader;
using WuDao.Common;

namespace WuDao.Content.Systems
{
    public class BladeTrailSystem : ModSystem
    {
        public override void OnWorldLoad()
        {
            BladeTrailRuntime.ClearServerRule();
            BladeTrailRuntime.TryRebuildFromServerConfig();
        }

        public override void OnWorldUnload()
        {
            BladeTrailRuntime.ClearServerRule();
        }
    }
}