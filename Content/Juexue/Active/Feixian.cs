// 天外飞仙：50 气，移除大部分减益 + 短时突进（无敌），并发出飞剑占位弹幕。
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using WuDao.Common;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Systems;

namespace WuDao.Content.Juexue.Active
{
    public class Feixian : JuexueItem
    {
        public override bool IsActive => true;
        public override int QiCost => 80;
        public override int SpecialCooldownTicks => 60 * 60; // 60s
        public const int FeixianFrameIndex = 9;
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            // 1) 清多数减益
            for (int i = 0; i < player.buffType.Length; i++)
            {
                int b = player.buffType[i];
                if (b <= 0) continue;
                if (Main.debuff[b])
                {
                    player.DelBuff(i);
                    i--;
                }
            }

            // 2) 记录目标点 & 启动直刺计时（在 QiPlayer.PreUpdate 里推进+无敌）
            qi.FeixianTarget = Main.MouseWorld;// ★ 记录目标
            qi.FeixianTicks = QiPlayer.FeixianTotalTicks;// ★ 启动计时
            if (!Main.dedServ)
            {
                qi.TriggerJuexueGhost(FeixianFrameIndex, durationTick: 60, scale: 1.1f, offset: new Vector2(0, -20));
            }
            // 3) 启动“飞仙定向冻结”：只冻结 NPC 与敌对弹幕，放行本玩家友方弹幕
            TimeStopSystem.StartFeixianFreeze(player.whoAmI, qi.FeixianTicks);

            // Vector2 p0 = player.Center, p1 = Main.MouseWorld;
            // qi.StartQiankunCurveDash(p0, (p0 + p1) * 0.5f /*c点在中点*/, p1, duration: QiPlayer.FeixianTotalTicks);
            // // 因为 c 在 p0-p1 直线上，弧高 H=0，自然就走直线

            // 4) 视觉：发射一枚“天顶剑”占位投射物（无需 tile 碰撞）
            // Vector2 dir = (player.Center - qi.FeixianTarget).SafeNormalize(Vector2.UnitX);
            // int proj = ProjectileID.FirstFractal; // ★ 天顶剑视觉
            // // 跟冲刺速度相同
            // int p = Projectile.NewProjectile(player.GetSource_ItemUse(Item), qi.FeixianTarget, dir * 26f, proj, 140, 4f, player.whoAmI);
            // Main.projectile[p].tileCollide = false;
            // Main.projectile[p].extraUpdates = 0;
            // Main.projectile[p].alpha = 100;

            return true;
        }
    }
}
