using Microsoft.Xna.Framework;
using Terraria;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Global;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Juexue.Active
{
    // 天外飞仙：移除大部分减益 + 短时突进（无敌），并发出飞剑弹幕。
    public class Feixian : JuexueItem
    {
        public override bool IsActive => true;
        public override int QiCost => 45;
        public override int SpecialCooldownTicks => 30 * 60; // 60s
        public const int FeixianFrameIndex = 9;
        // 伤害：240
        public const int Damage = 240;
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            if (player.whoAmI != Main.myPlayer)
                return false;

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
            // 在QiPlayer.PreUpdate 里推进+无敌
            qi.FeixianTicks = QiPlayer.FeixianTotalTicks;// ★ 启动计时
            if (!Main.dedServ)
            {
                // 冷却图标
                qi.TriggerJuexueCooldownIcon(
                    frameIndex: FeixianFrameIndex,
                    itemType: Type,                    // ModItem 的 Type
                    cooldownTicks: SpecialCooldownTicks,
                    scale: 1.1f,
                    offset: new Vector2(0, -20)
                );
            }
            
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                TimeStopSystem.TryStartFreeze(qi.FeixianTicks, 0, FreezeScope.Feixian, player.whoAmI);
            }
            else if (player.whoAmI == Main.myPlayer)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)MessageType.RequestTimeStop);
                packet.Write((byte)FreezeScope.Feixian);
                packet.Write(qi.FeixianTicks);
                packet.Write(0); // 飞仙不额外走这个系统冷却，仍由绝学冷却控制
                packet.Write((byte)player.whoAmI);
                packet.Send();
            }
            return true;
        }
    }
}
