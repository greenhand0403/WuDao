using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;

namespace WuDao.Content.Juexue.Active
{
    public class SkyWalking : JuexueItem
    {
        public override bool IsActive => true;
        public override int QiCost => 1; // 开启本身不扣，真正消耗在跳跃/踏空时发生
        public override int SpecialCooldownTicks => 30;

        public override bool TryActivate(Player player, QiPlayer qi)
        {
            if (qi.SkyWalkingActive)
            {
                qi.EndSkyWalking();
                // Main.NewText(Language.GetTextValue("Mods.WuDao.Items.SkyWalking.DisplayName"), Color.LightGray);
                return true;
            }

            // 骑乘时不允许开启月步
            if (player.mount != null && player.mount.Active)
            {
                return false;
            }

            qi.BeginSkyWalking();

            Main.NewText(DisplayName, Color.LightSkyBlue);
            return true;
        }
    }
}