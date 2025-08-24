using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using WuDao.Content.Global.Buffs;
using WuDao.Content.Items.Accessories;

namespace WuDao.Content.Cooldowns
{
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
            Player.AddBuff(ModContent.BuffType<RewinderCicadasCooldown>(), CooldownTicks);

            // 播个简洁特效（非必须）
            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item29, Player.Center); // 魔法“倒带”音效
            CombatText.NewText(Player.Hitbox, Color.Green, $"时间回溯恢复生命{healedAmount}");
            for (int i = 0; i < 25; i++)
            {
                int d = Dust.NewDust(Player.position, Player.width, Player.height, Terraria.ID.DustID.MagicMirror);
                Main.dust[d].velocity *= 1.5f;
                Main.dust[d].noGravity = true;
            }

            return false; // 阻止死亡（成功回溯）
        }

        //（可选）在人物 UI 上给个冷却提示：鼠标划过饰品时显示剩余冷却
        public override void PostUpdateEquips()
        {
            if (equipped && Main.LocalPlayer == Player && Main.mouseItem?.ModItem is RewinderCicadas)
            {
                // 当玩家正把饰品拿在鼠标上时，你也可以动态修改 Tooltip（需要 GlobalItem/ModifyTooltips 实现）
                // 这里不做演示，见下方 GlobalItem 可选实现。
            }
        }

        // 对外暴露一个查询方法（可用于 GlobalItem Tooltip）
        public int GetCooldownSecondsLeft()
        {
            long now = Main.GameUpdateCount;
            return (int)Math.Max(0, (nextReadyTick - now) / 60);
        }
    }

    public class TimeRewinderGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.ModItem is RewinderCicadas)
            {
                var mp = Main.LocalPlayer.GetModPlayer<RewinderCicadasPlayer>();
                int left = mp.GetCooldownSecondsLeft();
                if (left > 0)
                {
                    tooltips.Add(new TooltipLine(Mod, "CooldownLeft", $"冷却剩余：{left} 秒"));
                }
                else
                {
                    tooltips.Add(new TooltipLine(Mod, "CooldownReady", "冷却就绪"));
                }
            }
        }
    }
}
