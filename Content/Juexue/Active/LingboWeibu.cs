using Terraria;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using WuDao.Content.Config;
using Terraria.ModLoader;

namespace WuDao.Content.Juexue.Active
{
    // 凌波微步（开关技）：开启不收前置费用，每秒消耗气在 QiPlayer 里统一扣。
    // 再次按键关闭时不触发冷却，也不消耗气。
    public class LingboWeibu : JuexueItem
    {
        public override bool IsActive => true;
        public override int QiCost => 5;
        public override int SpecialCooldownTicks => 0; // 开启不需要专属冷却
        public const int LingboWeibuFrameIndex = 3;
        public override bool TryActivate(Player player, QiPlayer qi)
        {
            if (!JuexueRuntime.Enabled)
                return false;

            // 已开启 -> 直接关闭，不进 CD，不扣气
            if (qi.LingboActive)
            {
                qi.LingboActive = false;
                Main.NewText(DisplayName, Color.LightGray);
                return true;
            }
            if (!qi.TrySpendQi(QiCost)) return false;
            // 未开启 -> 正常走公共冷却时间检查
            if (!qi.CanUseActiveNow(Item.type, SpecialCooldownTicks))
            {
                Main.NewText(Language.GetTextValue("Mods.WuDao.Messages.JueXue.Cooldown"), Color.OrangeRed);
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
            qi.LingboQiCost = QiCost;
            Main.NewText(DisplayName, Color.SkyBlue);
            // —— 启动“凌波微步虚影” —— //
            if (!Main.dedServ)
            {
                // 冷却图标
                qi.TriggerJuexueCooldownIcon(
                    frameIndex: LingboWeibuFrameIndex,
                    itemType: Type,                    // ModItem 的 Type
                    cooldownTicks: SpecialCooldownTicks,
                    scale: 1.1f,
                    offset: new Vector2(0, -20)
                );
            }
            return true;
        }
    }
}
