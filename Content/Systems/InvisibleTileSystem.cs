using Terraria.ModLoader;
using WuDao.Content.Config;

namespace WuDao.Content.Systems
{
    // 染色刀光配置
    public class InvisibleTileSystem : ModSystem
    {
        public override void OnModLoad()
        {
            base.OnModLoad();
            InvisibleTileRuntime.TryRebuildFromConfig();
        }

        public override void PostAddRecipes()
        {
            // 从主菜单进入世界后确保应用最新配置
            InvisibleTileRuntime.TryRebuildFromConfig();
        }
        
    }
}
