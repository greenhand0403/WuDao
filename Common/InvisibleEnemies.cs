using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Config;

namespace WuDao.Common
{
    // 敌怪、弹幕和陷阱物块隐身
    public static class InvisibleEnemies
    {
        public static bool HasSpectreGoggles(Player player)
        {
            // armor[] 里包含盔甲、饰品、社交栏等（具体槽位分布不用死记，直接全扫最稳）
            for (int i = 3; i < 9; i++)
            {
                if (player.armor[i] != null && player.armor[i].type == ItemID.SpectreGoggles)
                    return true;
            }

            // 也可以把 miscEquips 扫一遍（一般不需要）
            return false;
        }

        public static bool CanSeeEcho(Player player)
        {
            if (player == null || !player.active)
                return false;

            var cfg = ModContent.GetInstance<WudaoConfig>();

            if (!cfg.InvisibleEnemies)
                return true;

            if (cfg.IgnoreSpectreGoggles)
                return false;

            // 先实现最核心的：佩戴幽灵护目镜
            if (HasSpectreGoggles(player))
                return true;

            return false;
        }
    }
}