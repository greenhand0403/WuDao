using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Buffs;
using WuDao.Content.Items.Accessories;

namespace WuDao.Content.Players
{
    // 春秋蝉：血量回溯系统
    public class RewinderCicadasPlayer : ModPlayer
    {
        // 配置
        private const int RewindDelayTicks = 60 * 3;       // 3 秒 = 180 tick
        private const int CooldownTicks = 60 * 300;     // 5 分钟 = 18000 tick
        private const int BufferSeconds = 6;            // 记录缓冲区长度（建议 > 3s）
        private const int ImmuneTicks = 60;           // 回溯后给予1秒无敌

        // 状态
        public bool equipped;
        private long nextReadyTick = 0;

        // 环形缓冲区：按帧记录生命值与时间戳
        private readonly int[] lifeRing = new int[60 * BufferSeconds];
        private readonly long[] tickRing = new long[60 * BufferSeconds];

        public override void ResetEffects()
        {
            equipped = false; // 每帧重置，由饰品设置为 true
        }

        public override void PostUpdate()
        {
            // 只有活着才记录
            if (!Player.dead)
            {
                int idx = (int)(Main.GameUpdateCount % lifeRing.Length);
                lifeRing[idx] = Player.statLife;
                tickRing[idx] = Main.GameUpdateCount;
            }
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (!equipped)
            {
                // 没有装备则正常死亡，减少1分钟冷却
                if (nextReadyTick > 60 * 60)
                {
                    nextReadyTick -= 60 * 60;
                }
                return true;
            }

            long now = Main.GameUpdateCount;

            // 冷却检查
            if (now < nextReadyTick)
                return true;

            // 取 3 秒前的快照
            long targetTick = now - RewindDelayTicks;
            if (targetTick <= 0)
                return true;

            int idx = (int)(targetTick % lifeRing.Length);
            // 确保这个槽位真的是目标时间记录（避免环覆盖/初始未填充）
            if (tickRing[idx] != targetTick)
                return true;

            int restoreLife = Math.Max(1, lifeRing[idx]);  // 至少回到 1 点
            int oldLife = Player.statLife;

            // 执行回溯：取消死亡
            Player.statLife = restoreLife;
            Player.dead = false;
            Player.immune = true;
            Player.immuneTime = ImmuneTicks;

            // 视觉治疗数字（以“增加量”显示）
            int healedAmount = Math.Max(0, restoreLife - Math.Max(0, oldLife));
            if (healedAmount > 0)
            {
                Player.HealEffect(healedAmount, true);
            }

            // 触发冷却
            nextReadyTick = now + CooldownTicks;

            // 可选：给一个冷却 Buff（如果你实现了，下方有 Buff 示例）
            Player.AddBuff(ModContent.BuffType<RewinderCicadasBuff>(), CooldownTicks);

            // 播个简洁特效（非必须）
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, Player.Center); // 魔法“倒带”音效
            CombatText.NewText(Player.Hitbox, Color.Green, Language.GetTextValue(
                "Mods.WuDao.Items.RewinderCicadas.Messages.Heal",
                healedAmount
            ));
            for (int i = 0; i < 25; i++)
            {
                int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.MagicMirror);
                Main.dust[d].velocity *= 1.5f;
                Main.dust[d].noGravity = true;
            }
            // ☆ 新增：回溯触发后，自动卸下并销毁春秋蝉（仅销毁1个已装备的功能位副本）
            ConsumeEquippedRewinder();
            return false; // 阻止死亡（成功回溯）
        }
        // 消耗饰品栏中已装备的春秋蝉
        private void ConsumeEquippedRewinder()
        {
            int cicadaType = ModContent.ItemType<RewinderCicadas>();

            // 功能饰品位在 armor[3..]，基础 8 格（3..10），加上 extraAccessorySlots
            int start = 3;
            int lastInclusive = 3 + 7 + Player.extraAccessorySlots;
            lastInclusive = Math.Min(lastInclusive, Player.armor.Length - 1);

            for (int i = start; i <= lastInclusive; i++)
            {
                ref Item slot = ref Player.armor[i];
                if (!slot.IsAir && slot.type == cicadaType && slot.accessory)
                {
                    // 卸下并销毁（TurnToAir = 从该格移除）
                    slot.TurnToAir();

                    // 立刻清掉本帧“已装备”标记，避免本帧重复逻辑
                    equipped = false;

                    // 一点点反馈（可选）
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Grass, Player.Center);
                    CombatText.NewText(Player.Hitbox, Color.LightGreen, Language.GetTextValue("Mods.WuDao.Items.RewinderCicadas.Messages.RewindTriggered"));

                    // 同步给其他玩家（多人联机时建议）
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        // 发送本玩家的饰品栏变动
                        NetMessage.SendData(MessageID.SyncEquipment, number: Player.whoAmI, number2: i);
                    }
                    return; // 只消耗一个
                }
            }
        }
    }
}
