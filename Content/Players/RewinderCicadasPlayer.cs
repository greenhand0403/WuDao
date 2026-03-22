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
                return true;

            if (Player.HasBuff(ModContent.BuffType<RewinderCicadasBuff>()))
                return true;

            long now = Main.GameUpdateCount;
            long targetTick = now - RewindDelayTicks;
            if (targetTick <= 0)
                return true;

            int idx = (int)(targetTick % lifeRing.Length);
            if (tickRing[idx] != targetTick)
                return true;

            int restoreLife = Math.Max(1, lifeRing[idx]);
            int oldLife = Player.statLife;

            Player.statLife = restoreLife;
            Player.dead = false;
            Player.immune = true;
            Player.immuneTime = ImmuneTicks;
            Player.AddBuff(ModContent.BuffType<RewinderCicadasBuff>(), CooldownTicks);

            int healedAmount = Math.Max(0, restoreLife - Math.Max(0, oldLife));
            if (healedAmount > 0)
                Player.HealEffect(healedAmount, true);

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, Player.Center);

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, Player.Center);
                CombatText.NewText(Player.Hitbox, Color.Green,
                    Language.GetTextValue("Mods.WuDao.Items.RewinderCicadas.Messages.Heal", healedAmount));

                for (int i = 0; i < 25; i++)
                {
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.MagicMirror);
                    Main.dust[d].velocity *= 1.5f;
                    Main.dust[d].noGravity = true;
                }
            }

            // 真正消耗饰品，优先让服务器/单机负责
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                ConsumeEquippedRewinder();
            }

            if (Main.netMode == NetmodeID.Server)
            {
                WuDao mod = ModContent.GetInstance<WuDao>();
                mod.BroadcastRewinderCicadasTriggered(Player.whoAmI, healedAmount);
            }
            return false;
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

                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendData(MessageID.SyncEquipment, number: Player.whoAmI, number2: i);
                    }
                    return;
                }
            }
        }
    }
}
