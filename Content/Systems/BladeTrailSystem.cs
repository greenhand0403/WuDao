using Terraria.ModLoader;
using WuDao.Common;

namespace WuDao.Content.Systems
{
    public class BladeTrailSystem : ModSystem
    {
        public override void OnWorldLoad()
        {
            BladeTrailRuntime.TryRebuildFromConfig();
        }

        public override void PostAddRecipes()
        {
            // 从主菜单进入世界后确保应用最新配置
            BladeTrailRuntime.TryRebuildFromConfig();
        }
    }
}
