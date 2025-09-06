// 凌波微步（开关技）：开启不收前置费用，每秒 5 气在 QiPlayer 里统一扣。
// 再次按键关闭时不触发冷却，也不消耗气。
using Terraria;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;

namespace WuDao.Content.Juexue.Active
{
    public class LingboWeibu : JuexueItem
    {
        public override JuexueID JuexueId => JuexueID.Active_LingboWeibu;
        public override bool IsActive => true;
        public override int QiCost => 0;
        public override int SpecialCooldownTicks => 0; // 开启不需要专属冷却

        public override bool TryActivate(Player player, QiPlayer qi)
        {
            // 已开启 -> 直接关闭，不进 CD，不扣气
            if (qi.LingboActive)
            {
                qi.LingboActive = false;
                Main.NewText("凌波微步：关闭", Microsoft.Xna.Framework.Color.LightGray);
                return true;
            }

            // 未开启 -> 正常走公共 2 秒冷却检查
            if (!qi.CanUseActiveNow(Item.type, SpecialCooldownTicks))
            {
                Main.NewText("绝学尚未冷却。", Microsoft.Xna.Framework.Color.OrangeRed);
                return false;
            }

            bool ok = OnActivate(player, qi);
            if (ok) qi.StampActiveUse(Item.type, SpecialCooldownTicks);
            return ok;
        }

        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            if (qi.QiMax <= 0) return false;
            qi.LingboActive = true;
            Main.NewText("凌波微步：开启（每秒消耗 15 气，10% 闪避）", Microsoft.Xna.Framework.Color.SkyBlue);
            return true;
        }
    }
}
