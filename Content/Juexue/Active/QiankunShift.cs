using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Content.Buffs;
using WuDao.Common;

namespace WuDao.Content.Juexue.Active
{
    // 乾坤大挪移
    public class QiankunShift : JuexueItem
    {
        public override int QiCost => 100;
        public override int SpecialCooldownTicks => 60 * 60; // 60 秒
        public const int QiankunShiftFrameIndex = 6;
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            Vector2 p0 = player.Center;
            Vector2 p1 = Main.MouseWorld;

            float dist = Vector2.Distance(p0, p1);
            if (dist < 40f) return false; // 太近不放

            // —— 生成弧线控制点：以中点为基准，沿法线偏移 —— //
            Vector2 mid = (p0 + p1) * 0.5f;
            Vector2 dir = p1 - p0;
            Vector2 nrm = dir.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2); // 右法线
            // 左/右随机选择一侧弧线（按住上/下也可以固定方向：按住上 = 左弧，按住下 = 右弧）
            int side = player.controlUp ? -1 : (player.controlDown ? 1 : (Main.rand.NextBool() ? 1 : -1));
            float offset = MathHelper.Clamp(dist * 0.45f, 120f, 360f); // 弧高
            Vector2 c = mid + nrm * offset * side;

            qi.StartQiankunCurveDash(p0, c, p1, 60);

            // 起手音效
            SoundEngine.PlaySound(SoundID.Item8, p0);
            if (!Main.dedServ)
            {
                // 触发 2 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
                // qi.TriggerJuexueGhost(QiankunShiftFrameIndex, durationTick: 30, scale: 1.1f, offset: new Vector2(0, -20));
                // 冷却图标
                qi.TriggerJuexueCooldownIcon(
                    frameIndex: QiankunShiftFrameIndex,
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
