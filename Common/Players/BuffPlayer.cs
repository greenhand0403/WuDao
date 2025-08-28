
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Buffs;

namespace WuDao.Common.Players
{
    /// <summary>
    /// 将所有 Buff 的数值效果集中到一个 ModPlayer 中，便于统一管理与调参。
    /// </summary>
    public class BuffPlayer : ModPlayer
    {
        // —— 战栗“手滑”控制 ——
        private int shiverLockTimer;               // 手滑锁定时长（帧）
        private int shiverSelectedItemCache = -1;  // 锁定期间固定的槽位

        // —— 天人五衰计时 ——
        private int fiveDecayTimer;

        public override void PreUpdate()
        {
            // 战栗锁定计时
            if (shiverLockTimer > 0)
            {
                shiverLockTimer--;
                // 锁定期间：禁止使用物品，强制保持原槽位
                Player.noItems = true;
            }
        }

        public override bool PreItemCheck()
        {
            // —— 金字塔雕像守护 ——
            if (Player.HasBuff(ModContent.BuffType<PyramidWard>()))
            {
                // —— 金字塔雕像：若获得金字塔守护 Buff，且站在实心方块上不移动，则额外 +50% 免伤
                if (Player.endurance < 0.45f)
                {
                    bool standingStill = Player.velocity.LengthSquared() < 0.01f &&
                                         !Player.controlLeft && !Player.controlRight &&
                                         !Player.controlUp && !Player.controlDown && !Player.controlJump;
                    // 粗略判断脚下是否有实心
                    bool solidBelow = IsStandingOnGround(Player);
                    if (standingStill && solidBelow)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            Dust.NewDust(Player.Center, Player.width, Player.height, DustID.Gold, 10, 10, 100, default, 1.5f);
                        }
                        Player.endurance += 0.50f;
                    }
                }
            }
            // 战栗：80% 概率“手滑” -> 锁定 120 帧（2 秒），模拟掉落后暂不可用的体验
            if (Player.HasBuff(ModContent.BuffType<Shiver>()) && shiverLockTimer <= 0)
            {
                // 只在尝试使用物品且当前没有动画时判定一次，避免每帧多次触发
                if (Player.controlUseItem && Player.itemAnimation == 0 && Player.HeldItem?.type > ItemID.None)
                {
                    if (Main.rand.NextFloat() < 0.50f)
                    {
                        shiverLockTimer = 120;
                        shiverSelectedItemCache = Player.selectedItem;
                        // 提示文本（仅本地）
                        if (Player.whoAmI == Main.myPlayer)
                            CombatText.NewText(Player.getRect(), new Color(180, 220, 255), $"{shiverSelectedItemCache}手滑！", dramatic: true);

                        Player.direction = -Player.direction;
                        Player.DropSelectedItem();
                        Player.direction = -Player.direction;
                        return false;
                    }
                }
            }
            return true;
        }
        public override void PostUpdateBuffs()
        {
            // —— 寻死：防御 -90%（只保留 10%）
            if (Player.HasBuff(ModContent.BuffType<Suicidal>()))
            {
                Player.statDefense *= 0.1f;
            }

            // —— 软弱：所有伤害 -90%
            if (Player.HasBuff<Weakness>())
            {
                Player.GetDamage(DamageClass.Generic) *= 0.10f;
            }

            // —— 心灵枯萎：与生命诅咒类似
            if (Player.HasBuff<WitheredMind>())
            {
                Player.manaRegen = 1;
                if (Player.statMana > Player.statManaMax) Player.statMana -= 1;
            }

            // —— 笨拙：暴击率 -90%
            if (Player.HasBuff<Clumsy>())
            {
                Player.GetCritChance(DamageClass.Generic) *= 0.10f;
            }

            // —— 生命诅咒：基于最大生命的 10%/秒持续伤害（lifeRegen 为每 0.5s 结算一次，故 *2）
            if (Player.HasBuff<LifeCurse>())
            {
                int drain = Player.statLifeMax2 / 10 * 2; // lifeRegen 单位换算
                if (Player.lifeRegen > 0) Player.lifeRegen = 0;
                Player.lifeRegenTime = 0;
                Player.lifeRegen -= drain;
            }

            // —— 拖延症 / 急性子：移动速度 ±90%
            if (Player.HasBuff<Procrastination>())
            {
                Player.moveSpeed *= 0.10f;
                Player.maxRunSpeed *= 0.10f;
                Player.runAcceleration *= 0.10f;
            }
            if (Player.HasBuff<Impatient>())
            {
                Player.moveSpeed *= 1.90f;
                Player.maxRunSpeed *= 1.90f;
                Player.runAcceleration *= 1.90f;
            }

            // —— 幻想破灭：强力压制常见数值收益（易实现版，避免全局拦截饰品流程导致兼容问题）
            if (Player.HasBuff<BrokenDreams>())
            {
                Player.GetDamage(DamageClass.Generic) *= 0.75f;
                Player.GetCritChance(DamageClass.Generic) *= 0.75f;
                Player.endurance -= 0.10f; // 免伤 -10%
                Player.moveSpeed *= 0.85f;
                if (Player.endurance < -0.90f) Player.endurance = -0.90f; // 下限保护
            }

            // —— 武器巨大化：近战尺寸 +20%（在 ModifyWeaponScale 内实现，这里无需处理）

            // —— 收藏家：武器稀有度 => 伤害；饰品稀有度总和 => 防御
            if (Player.HasBuff<Collector>())
            {
                // 武器稀有度（默认 rare ∈ [-1, ...]，负值按 0 处理）
                int r = Math.Max(0, Player.HeldItem?.rare ?? 0);
                float dmgBonus = 1f + 0.02f * r; // 每稀有度 +2% 伤害
                Player.GetDamage(DamageClass.Generic) *= dmgBonus;

                // 饰品稀有度总和 * 0.5 防御
                int sum = 0;
                for (int i = 3; i < 10 + Player.extraAccessorySlots; i++)
                {
                    Item acc = Player.armor[i];
                    if (acc != null && !acc.IsAir && acc.accessory)
                        sum += Math.Max(0, acc.rare);
                }
                Player.statDefense += (int)Math.Floor(sum * 0.5f);
            }

            // —— 资本主义：计算总金币（背包 + 4 个存钱容器）
            if (Player.HasBuff<Capitalism>())
            {
                long copper = CountAllCoins(Player);
                const long TenPlatinum = 1000000L * 10L; // 10 铂 = 10,000,000 铜
                if (copper >= TenPlatinum)
                {
                    Player.GetDamage(DamageClass.Generic) *= 1.10f;
                    Player.endurance += 0.10f;
                }
                else
                {
                    Player.GetDamage(DamageClass.Generic) *= 0.90f;
                    Player.endurance -= 0.10f;
                    if (Player.endurance < -0.90f) Player.endurance = -0.90f; // 下限保护
                }
            }

            // —— 天人五衰：每 120 帧随机再附加 1 个子减益
            if (Player.HasBuff<FiveDecay>())
            {
                fiveDecayTimer++;
                if (fiveDecayTimer >= 120)
                {
                    fiveDecayTimer = 0;
                    int[] pool = new int[] {
                        ModContent.BuffType<Suicidal>(),
                        ModContent.BuffType<Weakness>(),
                        ModContent.BuffType<WitheredMind>(),
                        ModContent.BuffType<Clumsy>(),
                        ModContent.BuffType<Shiver>()
                    };
                    int pick = Main.rand.Next(pool.Length);
                    Player.AddBuff(pool[pick], 180); // 3 秒
                }
            }
            else
            {
                fiveDecayTimer = 0;
            }
        }
        // 更稳的“是否站在实心地面上”判断（支持半砖/斜坡/台阶等）
        public bool IsStandingOnGround(Player p, int probePixelsDown = 4)
        {
            // 从角色脚底（position + height）开始，向下探测 probePixelsDown 高度、宽度等于玩家宽度的矩形
            // 只要矩形里有任何可碰撞瓦块，就返回 true
            // 注意：probePixelsDown 不要太大，避免站在台阶旁边时把远处地面也算进来，6~8px 比较合适
            Vector2 probeTopLeft = new Vector2(p.position.X, p.position.Y + p.height);
            bool hasSolid = Collision.SolidCollision(probeTopLeft, p.width, probePixelsDown);
            return hasSolid;
        }
        public override void ModifyWeaponCrit(Item item, ref float crit)
        {
            // 「笨拙」的暴击已经在 PostUpdateBuffs 里用 Generic 处理，这里可不做额外处理
            // 这里保留钩子，方便未来扩展。
        }
        public override void ModifyItemScale(Item item, ref float scale)
        {
            if (Player.HasBuff<SacrificialBranding>()
                && item.DamageType == DamageClass.Melee)
            {
                scale *= 1.20f;
            }
        }
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (Player.HasBuff<SacrificialBranding>()
                && npc.boss)
            {
                modifiers.FinalDamage *= 1.5f;
            }
        }
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (Player.HasBuff<GiantWeapon>()
                && Main.npc[proj.whoAmI].boss)
            {
                modifiers.FinalDamage *= 1.5f;
            }
        }
        private static long CountAllCoins(Player p)
        {
            long total = 0;
            void Count(Item i)
            {
                if (i == null || i.IsAir) return;
                if (i.type == ItemID.CopperCoin) total += i.stack * 1L;
                else if (i.type == ItemID.SilverCoin) total += i.stack * 100L;
                else if (i.type == ItemID.GoldCoin) total += i.stack * 10000L;
                else if (i.type == ItemID.PlatinumCoin) total += i.stack * 1000000L;
            }
            foreach (var it in p.inventory) Count(it);
            foreach (var it in p.bank?.item ?? Array.Empty<Item>()) Count(it);
            foreach (var it in p.bank2?.item ?? Array.Empty<Item>()) Count(it);
            foreach (var it in p.bank3?.item ?? Array.Empty<Item>()) Count(it);
            foreach (var it in p.bank4?.item ?? Array.Empty<Item>()) Count(it);
            return total;
        }
    }
}
