// 利刃华尔兹：120 气，30s 冷却。启动后由 QiPlayer 的 BladeWaltz* 流程驱动 8 段攻击。
using Terraria;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Juexue.Active
{
    public class BladeWaltz : JuexueItem
    {
        public override bool IsActive => true;
        public override int QiCost => 99;
        public override int SpecialCooldownTicks => 60 * 1; // 120s
        public const int BladeWaltzFrameIndex = 10;
        public override bool TryActivate(Player player, QiPlayer qi)
        {
            // 进行中：静默忽略
            if (qi.BladeWaltzTicks > 0)
                return false;

            // 冷却检查
            if (!qi.CanUseActiveNow(Item.type, SpecialCooldownTicks))
            {
                Main.NewText("绝学尚未冷却。",Color.OrangeRed);
                return false;
            }

            // 启动时仅扣一次
            if (!qi.TrySpendQi(QiCost))
            {
                Main.NewText("气力不足！",Color.OrangeRed);
                return false;
            }

            // 8 段流程（每段 ~0.9s）
            qi.BladeWaltzHitsLeft = 8;
            qi.BladeWaltzStepTimer = 0;      // 立刻进入第一段
            qi.BladeWaltzTicks = 8 * 48;     // 8x0.8s
            qi.BladeWaltzTarget = -1;

            player.RemoveAllGrapplingHooks();

            // 盖章冷却（含 2s 公共冷却）
            qi.StampActiveUse(Item.type, SpecialCooldownTicks);

            Main.NewText("利刃华尔兹！", Color.MediumPurple);
            // if (!Main.dedServ)
            // {
            //     // 触发 1 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
            //     qi.TriggerJuexueGhost(BladeWaltzFrameIndex, durationTick: 180, scale: 1.1f, offset: new Vector2(0, -20));
            // }
            return true;
        }
    }
}
