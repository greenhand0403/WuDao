using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Config;

namespace WuDao.Common
{
    // 敌怪、弹幕和陷阱物块隐身
    public static class InvisibleEnemies
    {
        public class AccessoryFlagsPlayer : ModPlayer
        {
            public bool hasSpectreGoggles;

            public override void ResetEffects()
            {
                hasSpectreGoggles = false;
            }
        }
        public class VanillaAccessoryGlobalItem : GlobalItem
        {
            public override void UpdateAccessory(Item item, Player player, bool hideVisual)
            {
                if (item.type == ItemID.SpectreGoggles)
                {
                    player.GetModPlayer<AccessoryFlagsPlayer>().hasSpectreGoggles = true;
                }
            }
        }
        public static bool HasSpectreGoggles(Player player)
        {
            bool equipped = player.GetModPlayer<AccessoryFlagsPlayer>().hasSpectreGoggles;
            return equipped;
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