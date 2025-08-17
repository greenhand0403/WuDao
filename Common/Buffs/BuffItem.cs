using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items.Weapons.Melee;
using WuDao.Content.Items.Accessories;
using WuDao.Content.Global.NPCs;

// ----------------------------------------------------------
// 可复用的 Buff + 属性（速度/回血等）统一框架
// 目标：
// 1) Buff：只在“需要时”刷新（低开销顶时间）
// 2) 属性：按帧累计到 ModPlayer，再一次性应用（避免重复叠加/便于统计）
// 3) 支持：手持、饰品、装备（护甲）、以及环境/时间等条件
// 4) 两种接入方式：继承基类 BuffItem，或实现接口 IBuffItemProvider / IStatItemProvider
// ----------------------------------------------------------

namespace WuDao.Common.Buffs
{
    #region —— 条件工具（可组合） ——
    public static class BuffConditions
    {
        public static Func<Player, Item, bool> Always => (p, i) => true;
        public static Func<Player, Item, bool> Never => (p, i) => false;

        // 组合器
        public static Func<Player, Item, bool> And(this Func<Player, Item, bool> a, Func<Player, Item, bool> b)
            => (p, i) => a(p, i) && b(p, i);
        public static Func<Player, Item, bool> Or(this Func<Player, Item, bool> a, Func<Player, Item, bool> b)
            => (p, i) => a(p, i) || b(p, i);
        public static Func<Player, Item, bool> Not(this Func<Player, Item, bool> inner)
            => (p, i) => !inner(p, i);

        // 生物群落/环境（根据 Player.Zone* 标志）
        public static Func<Player, Item, bool> InOcean => (p, i) => p.ZoneBeach;
        public static Func<Player, Item, bool> InDesert => (p, i) => p.ZoneDesert || p.ZoneUndergroundDesert;
        public static Func<Player, Item, bool> InJungle => (p, i) => p.ZoneJungle;
        public static Func<Player, Item, bool> InSnow => (p, i) => p.ZoneSnow;
        public static Func<Player, Item, bool> InCorrupt => (p, i) => p.ZoneCorrupt;
        public static Func<Player, Item, bool> InCrimson => (p, i) => p.ZoneCrimson;
        public static Func<Player, Item, bool> InHallow => (p, i) => p.ZoneHallow;
        public static Func<Player, Item, bool> InGlowshroom => (p, i) => p.ZoneGlowshroom;

        // “森林” 的近似判断
        public static Func<Player, Item, bool> InForest => (p, i) =>
            p.ZoneOverworldHeight &&
            !p.ZoneDesert && !p.ZoneUndergroundDesert && !p.ZoneJungle && !p.ZoneSnow &&
            !p.ZoneHallow && !p.ZoneCrimson && !p.ZoneCorrupt && !p.ZoneGlowshroom &&
            !p.ZoneBeach && !p.ZoneMeteor && !p.ZoneGranite && !p.ZoneMarble &&
            !p.ZoneDungeon && !p.ZoneOldOneArmy;

        // 时间/高度
        public static Func<Player, Item, bool> DayTime => (p, i) => Main.dayTime;
        public static Func<Player, Item, bool> NightTime => (p, i) => !Main.dayTime;
        public static Func<Player, Item, bool> InSkyHeight => (p, i) => p.ZoneSkyHeight;
        public static Func<Player, Item, bool> InOverworld => (p, i) => p.ZoneOverworldHeight;
        public static Func<Player, Item, bool> InUnderworld => (p, i) => p.ZoneUnderworldHeight;

        // 自定义
        public static Func<Player, Item, bool> FromPlayerPredicate(Func<Player, bool> pred) => (p, i) => pred(p);
    }
    #endregion

    #region —— Buff 规则 ——
    public struct BuffEffect
    {
        public int BuffId;           // BuffID.* 或 ModContent.BuffType<T>()
        public int TopUpAmount;      // 一次性顶到的时间（帧），推荐 60~300
        public int RefreshThreshold; // 当剩余时间低于该阈值（帧）时刷新
        public bool Quiet;           // 是否静默（不弹提示）

        public BuffEffect(int buffId, int topUpAmount = 120, int refreshThreshold = 30, bool quiet = true)
        {
            BuffId = buffId;
            TopUpAmount = topUpAmount;
            RefreshThreshold = refreshThreshold;
            Quiet = quiet;
        }
    }

    public class BuffRule
    {
        public Func<Player, Item, bool> Condition;
        public readonly List<BuffEffect> Effects = new();

        public BuffRule(Func<Player, Item, bool> condition, params BuffEffect[] effects)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Effects.AddRange(effects);
        }

        public BuffRule Add(BuffEffect effect)
        {
            Effects.Add(effect);
            return this;
        }
    }
    #endregion

    #region —— 属性（速度/回血/最大生命等）规则 ——
    // 累计槽：所有道具把“本帧的属性改动”累加到这里，再由 ModPlayer 统一应用。
    public class BuffStatPlayer : ModPlayer
    {
        // 常用属性累加器
        public float MoveSpeedAdd;      // 加法（与 vanilla Player.moveSpeed 同维度）
        public float MoveSpeedMult = 1; // 乘法
        public float RunSpeedAdd;       // 叠加到 Player.maxRunSpeed
        public float RunAccelAdd;       // 叠加到 Player.runAcceleration
        public float JumpSpeedAdd;      // 叠加到 Player.jumpSpeedBoost
        public int LifeRegenAdd;        // 叠加到 Player.lifeRegen（约每秒回复 = lifeRegen/2 生命）
        public float LifeRegenMult = 1; // 生命再生“倍率”
        public int DefenseAdd;          // 叠加防御
        public int MaxLifeAdd;          // 叠加最大生命（作用于 statLifeMax2）
        public float MaxLifeMult = 1;   // 百分比型最大生命加成（乘到 statLifeMax2）
        public float MeleeSizeMult = 1f;
        public readonly HashSet<int> ImmuneBuffs = new();
        public float CritGenericAdd, CritMeleeAdd, CritRangedAdd, CritMagicAdd, CritSummonAdd;
        public bool FlagSlowFall, FlagNoFallDmg;
        // 累加槽（放在已有字段旁）
        public int MaxManaAdd;
        public float MaxManaMult = 1f;
        public int ManaRegenAdd;
        public float ManaRegenMult = 1f;

        // BuffStatPlayer 字段
        public float AttackSpeedAddGeneric = 0;
        public float DamageMultAddGeneric = 0;
        public float AttackSpeedMultGeneric = 1f;   // 攻速乘法
        public float DamageMultGeneric = 1f;        // 伤害乘法
        public float DefenseMult = 1f;           // 防御倍率（在加法 DefenseAdd 之前乘）

        public override void ResetEffects()
        {
            MoveSpeedAdd = 0f; MoveSpeedMult = 1f;
            RunSpeedAdd = 0f; RunAccelAdd = 0f; JumpSpeedAdd = 0f;
            LifeRegenAdd = 0; LifeRegenMult = 1f;
            DefenseAdd = 0; MaxLifeAdd = 0; MaxLifeMult = 1f; MeleeSizeMult = 1f;
            ImmuneBuffs.Clear();
            CritGenericAdd = CritMeleeAdd = CritRangedAdd = CritMagicAdd = CritSummonAdd = 0f;
            FlagSlowFall = FlagNoFallDmg = false;
            MaxManaAdd = 0; MaxManaMult = 1f;
            ManaRegenAdd = 0; ManaRegenMult = 1f;
            AttackSpeedAddGeneric = 0f;
            DamageMultAddGeneric = 0f;
            AttackSpeedMultGeneric = 1f;
            DamageMultGeneric = 1f;
            DefenseMult = 1f;
        }
        public override bool PreModifyLuck(ref float luck)
        {
            // 先应用移动相关
            Player.moveSpeed += MoveSpeedAdd;
            Player.moveSpeed *= MoveSpeedMult;
            Player.maxRunSpeed += RunSpeedAdd;
            Player.runAcceleration += RunAccelAdd;
            return base.PreModifyLuck(ref luck);
        }
        // BuffStatPlayer 内新增
        internal void ApplyMovementNow()
        {
            Player.moveSpeed += MoveSpeedAdd;
            Player.moveSpeed *= MoveSpeedMult;
            Player.maxRunSpeed += RunSpeedAdd;
            Player.runAcceleration += RunAccelAdd;
        }

        // BuffStatPlayer 内新增字段（缓存用，避免每帧分配）
        private static readonly List<StatRule> _tmpRules = new();

        public override void PostUpdateRunSpeeds()
        {
            // 1) 先把当前累加槽清零（只用于本阶段立即应用）
            MoveSpeedAdd = 0f; MoveSpeedMult = 1f;
            RunSpeedAdd = 0f; RunAccelAdd = 0f;

            // 2) 动态从“手持物品”拉取规则（不特判类型）
            Item held = Player.HeldItem;
            if (held?.ModItem is IStatItemProvider statProvider)
            {
                _tmpRules.Clear();
                statProvider.AddStatRules(Player, held, _tmpRules);

                // 3) 仅筛选满足条件的规则，并把“移动三件套”写入累加槽
                foreach (var rule in _tmpRules)
                {
                    if (rule?.Condition == null || !rule.Condition(Player, held))
                        continue;

                    foreach (var eff in rule.Effects)
                        eff.Apply(Player, this); // 这一步会把移动相关增量写进 MoveSpeed*/Run* 字段
                }
            }

            // 4) 当场应用“移动三件套”（影响本帧速度计算）
            ApplyMovementNow();
        }
        public override void PostItemCheck()
        {
            Player.statManaMax2 = (int)Math.Round(Player.statManaMax2 * MaxManaMult) + MaxManaAdd;
            if (Player.statMana > Player.statManaMax2) Player.statMana = Player.statManaMax2;

            Player.manaRegen = (int)Math.Round(Player.manaRegen * ManaRegenMult) + ManaRegenAdd;

            // 生命再生：先乘再加，便于和其他来源组合
            Player.lifeRegen = (int)Math.Round(Player.lifeRegen * LifeRegenMult);
            Player.lifeRegen += LifeRegenAdd;
            // 先乘
            Player.GetAttackSpeed(DamageClass.Generic) *= AttackSpeedMultGeneric;
            Player.GetAttackSpeed(DamageClass.Generic) += AttackSpeedAddGeneric;

            Player.GetDamage(DamageClass.Generic) *= DamageMultGeneric;
            Player.GetDamage(DamageClass.Generic) += DamageMultAddGeneric;

            Player.jumpSpeedBoost += JumpSpeedAdd;

            Player.statDefense *= DefenseMult;
            Player.statDefense += DefenseAdd;
            // 最大生命：先百分比，再平移
            int newMaxLife = (int)Math.Round(Player.statLifeMax2 * MaxLifeMult) + MaxLifeAdd;
            if (newMaxLife < 1) newMaxLife = 1;
            Player.statLifeMax2 = newMaxLife;
            if (Player.statLife > Player.statLifeMax2)
                Player.statLife = Player.statLifeMax2;

            Player.GetCritChance(DamageClass.Generic) += CritGenericAdd;
            Player.GetCritChance(DamageClass.Melee) += CritMeleeAdd;
            Player.GetCritChance(DamageClass.Ranged) += CritRangedAdd;
            Player.GetCritChance(DamageClass.Magic) += CritMagicAdd;
            Player.GetCritChance(DamageClass.Summon) += CritSummonAdd;

            // 应用本帧声明的免疫
            foreach (var buffId in ImmuneBuffs)
            {
                Player.buffImmune[buffId] = true;
                if (Player.HasBuff(buffId))
                    Player.ClearBuff(buffId);
            }
            if (FlagSlowFall)
            {
                Player.slowFall = true;
            }
            if (FlagNoFallDmg)
            {
                Player.noFallDmg = true;
            }
        }
    }
    // 一个“属性效果”的表示：
    public delegate void StatApplier(Player player, BuffStatPlayer acc);

    public readonly struct StatEffect
    {
        public readonly StatApplier Apply;
        public StatEffect(StatApplier apply) => Apply = apply ?? throw new ArgumentNullException(nameof(apply));

        // 常用工厂方法（可按需继续扩展）
        public static StatEffect MoveSpeed(float add = 0f, float mult = 1f) => new((p, acc) => { acc.MoveSpeedAdd += add; acc.MoveSpeedMult *= mult; });
        public static StatEffect RunSpeed(float add) => new((p, acc) => acc.RunSpeedAdd += add);
        public static StatEffect RunAcceleration(float add) => new((p, acc) => acc.RunAccelAdd += add);
        public static StatEffect JumpSpeed(float add) => new((p, acc) => acc.JumpSpeedAdd += add);
        public static StatEffect LifeRegen(int add) => new((p, acc) => acc.LifeRegenAdd += add);
        public static StatEffect LifeRegenMultiplier(float mult) => new((p, acc) => acc.LifeRegenMult *= mult);
        public static StatEffect LifeRegenPercent(float percent) => new((p, acc) => acc.LifeRegenMult *= 1f + percent);
        public static StatEffect Defense(int add) => new((p, acc) => acc.DefenseAdd += add);
        public static StatEffect MaxLife(int add) => new((p, acc) => acc.MaxLifeAdd += add);

        public static StatEffect MaxLifeMultiplier(float mult) => new((p, acc) => acc.MaxLifeMult *= mult);
        public static StatEffect MaxLifePercent(float percent) => new((p, acc) => acc.MaxLifeMult *= 1f + percent);
        public static StatEffect MeleeSizePercent(float percent) => new((p, acc) => acc.MeleeSizeMult *= 1f + percent);
        public static StatEffect ImmuneTo(params int[] buffIds) => new((p, acc) => { foreach (var id in buffIds) acc.ImmuneBuffs.Add(id); });
        public static StatEffect Crit(float add) => new((p, acc) => acc.CritGenericAdd += add);
        public static StatEffect MeleeCrit(float add) => new((p, acc) => acc.CritMeleeAdd += add);
        public static StatEffect RangedCrit(float add) => new((p, acc) => acc.CritRangedAdd += add);
        public static StatEffect MagicCrit(float add) => new((p, acc) => acc.CritMagicAdd += add);
        public static StatEffect SummonCrit(float add) => new((p, acc) => acc.CritSummonAdd += add);
        public static StatEffect SlowFall() => new((p, acc) => acc.FlagSlowFall = true);
        public static StatEffect NoFallDmg() => new((p, acc) => acc.FlagNoFallDmg = true);
        // 法力
        public static StatEffect MaxMana(int add) => new((p, acc) => acc.MaxManaAdd += add);
        public static StatEffect MaxManaMultiplier(float mult) => new((p, acc) => acc.MaxManaMult *= mult);
        public static StatEffect MaxManaPercent(float percent) => new((p, acc) => acc.MaxManaMult *= 1f + percent);
        public static StatEffect ManaRegen(int add) => new((p, acc) => acc.ManaRegenAdd += add);
        public static StatEffect ManaRegenMultiplier(float mult) => new((p, acc) => acc.ManaRegenMult *= mult);
        public static StatEffect ManaRegenPercent(float percent) => new((p, acc) => acc.ManaRegenMult *= 1f + percent);

        // 攻速 & 伤害（对所有类型）
        public static StatEffect AttackSpeedPercent(float percent) => new((p, acc) => acc.AttackSpeedAddGeneric += percent);
        public static StatEffect DamagePercent(float percent) => new((p, acc) => acc.DamageMultAddGeneric += percent);

        // 防御倍率
        public static StatEffect DefenseMultiplier(float mult) => new((p, acc) => acc.DefenseMult *= mult);
        public static StatEffect DefensePercent(float percent) => new((p, acc) => acc.DefenseMult *= 1f + percent);

        public static StatEffect AttackSpeedMultiplier(float mult) => new((p, acc) => acc.AttackSpeedMultGeneric *= mult);

        public static StatEffect DamageMultiplier(float mult) => new((p, acc) => acc.DamageMultGeneric *= mult);

    }

    public class StatRule
    {
        public Func<Player, Item, bool> Condition;
        public readonly List<StatEffect> Effects = new();

        public StatRule(Func<Player, Item, bool> condition, params StatEffect[] effects)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Effects.AddRange(effects);
        }

        public StatRule Add(StatEffect effect)
        {
            Effects.Add(effect);
            return this;
        }
    }
    #endregion

    #region —— 接口 & 抽象基类 ——
    // 任何实现了此接口的 ModItem，都可以通过 GlobalItem 自动应用【Buff】规则。
    public interface IBuffItemProvider
    {
        void AddBuffRules(Player player, Item item, IList<BuffRule> rules);
    }

    // 任何实现了此接口的 ModItem，都可以通过 GlobalItem 自动应用【属性】规则。
    public interface IStatItemProvider
    {
        void AddStatRules(Player player, Item item, IList<StatRule> rules);
    }

    // 方便继承用的基类：既能写 Buff，也能写属性。
    public abstract class BuffItem : ModItem, IBuffItemProvider, IStatItemProvider
    {
        public sealed override void HoldItem(Player player) { }
        public sealed override void UpdateAccessory(Player player, bool hideVisual) { }
        public sealed override void UpdateEquip(Player player) { }

        public void AddBuffRules(Player player, Item item, IList<BuffRule> rules) => BuildBuffRules(player, item, rules);
        public void AddStatRules(Player player, Item item, IList<StatRule> rules) => BuildStatRules(player, item, rules);

        protected virtual void BuildBuffRules(Player player, Item item, IList<BuffRule> rules) { }
        protected virtual void BuildStatRules(Player player, Item item, IList<StatRule> rules) { }
    }
    #endregion

    #region —— 全局实现：统一收集 & 应用 ——
    public class BuffItemGlobal : GlobalItem
    {
        private static readonly List<BuffRule> _buffRules = new();
        private static readonly List<StatRule> _statRules = new();

        public override void HoldItem(Item item, Player player) => TryApplyFromItem(player, item);
        public override void UpdateEquip(Item item, Player player)
        {
            // 避免饰品在 UpdateEquip 中再次应用（饰品应只走 UpdateAccessory）
            if (item.accessory) return;
            TryApplyFromItem(player, item);
        }

        public override void ModifyItemScale(Item item, Player player, ref float scale)
        {
            if (item.CountsAsClass(DamageClass.Melee))
            {
                var acc = player.GetModPlayer<BuffStatPlayer>();
                scale *= acc.MeleeSizeMult;   // 你框架里定义的尺寸倍率，默认 1f
            }
        }
        private static void TryApplyFromItem(Player player, Item item)
        {
            if (item?.ModItem == null) return;

            // Buff 规则
            if (item.ModItem is IBuffItemProvider buffProvider)
            {
                _buffRules.Clear();
                buffProvider.AddBuffRules(player, item, _buffRules);
                ApplyBuffRulesSmart(player, item, _buffRules);
            }

            // 属性规则
            if (item.ModItem is IStatItemProvider statProvider)
            {
                _statRules.Clear();
                statProvider.AddStatRules(player, item, _statRules);
                ApplyStatRules(player, item, _statRules);
            }
        }
        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (hideVisual) return; // 社交饰品不生效，防止再次叠加

            TryApplyFromItem(player, item); // 你已有的收集规则调用

            // 如果这个 item 是“全面退化”饰品，就给 ModPlayer 打标
            if (item.type == ModContent.ItemType<DevolutionCharm>())
            {
                player.GetModPlayer<DevolutionPlayer>().HasDevolutionAura = true;
            }
            else if (item.type == ModContent.ItemType<WrathLotus>())
            {
                player.GetModPlayer<WrathLotusPlayer>().hasLotus = true;
            }
        }

        private static void ApplyBuffRulesSmart(Player player, Item item, List<BuffRule> rules)
        {
            for (int r = 0; r < rules.Count; r++)
            {
                var rule = rules[r];
                if (rule?.Condition == null || !rule.Condition(player, item)) continue;
                for (int e = 0; e < rule.Effects.Count; e++)
                    ApplyBuffSmart(player, rule.Effects[e]);
            }
        }

        // 低开销顶时间（仅在需要时操作）
        private static void ApplyBuffSmart(Player player, in BuffEffect eff)
        {
            int idx = player.FindBuffIndex(eff.BuffId);
            if (idx == -1)
                player.AddBuff(eff.BuffId, eff.TopUpAmount, eff.Quiet);
            else if (player.buffTime[idx] < eff.RefreshThreshold)
                player.buffTime[idx] = eff.TopUpAmount;
        }

        private static void ApplyStatRules(Player player, Item item, List<StatRule> rules)
        {
            var acc = player.GetModPlayer<BuffStatPlayer>();
            for (int r = 0; r < rules.Count; r++)
            {
                var rule = rules[r];
                if (rule?.Condition == null || !rule.Condition(player, item)) continue;
                for (int e = 0; e < rule.Effects.Count; e++)
                    rule.Effects[e].Apply(player, acc);
            }
        }
    }
    #endregion
}

// ----------------------------------------------------------
// 使用示例
// 1) 武器：手持时 → 快乐(Happy) + 涌动之歌(Well Fed)；同时 +10% 移速，+1 生命再生
// 2) 饰品：在海洋时 → 水下呼吸；在“森林”时 → Swiftness；夜晚时 → +0.5 跑速、+0.08 跑步加速度
// 3) 生命相关：+40 最大生命；生命再生 +50%（倍率）并额外 +2（平移）
// ----------------------------------------------------------
/*
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using YourMod.Common.Buffs; // 引用上面的命名空间

// 示例 1：武器（继承 BuffItem，既写 Buff 又写属性）
public class ExampleHappyWeapon : BuffItem
{
    protected override void BuildBuffRules(Player player, Item item, IList<BuffRule> rules)
    {
        rules.Add(new BuffRule(BuffConditions.Always,
            new BuffEffect(BuffID.Happy, topUpAmount: 180, refreshThreshold: 30),
            new BuffEffect(BuffID.WellFed, topUpAmount: 180, refreshThreshold: 30)
        ));
    }

    protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
    {
        rules.Add(new StatRule(BuffConditions.Always,
            StatEffect.MoveSpeed(mult: 1.10f),   // +10% 移速
            StatEffect.LifeRegen(1)               // +1 lifeRegen（= 约每秒 +0.5 血）
        ));
    }
}

// 示例 2：饰品（实现接口，按条件给 Buff + 属性）
public class ExampleOceanForestAccessory : ModItem, IBuffItemProvider, IStatItemProvider
{
    public override void SetDefaults()
    {
        Item.accessory = true;
    }

    public void AddBuffRules(Player player, Item item, IList<BuffRule> rules)
    {
        // 海洋：水下呼吸（Gills）
        rules.Add(new BuffRule(BuffConditions.InOcean,
            new BuffEffect(BuffID.Gills, topUpAmount: 300, refreshThreshold: 60)
        ));

        // “森林”：疾行药水（Swiftness）
        rules.Add(new BuffRule(BuffConditions.InForest,
            new BuffEffect(BuffID.Swiftness, topUpAmount: 300, refreshThreshold: 60)
        ));
    }

    public void AddStatRules(Player player, Item item, IList<StatRule> rules)
    {
        // 夜晚的小幅移动强化 + 生命相关示例
        rules.Add(new StatRule(BuffConditions.NightTime,
            StatEffect.RunSpeed(0.5f),        // maxRunSpeed +0.5
            StatEffect.RunAcceleration(0.08f),// runAcceleration +0.08
            StatEffect.MaxLife(40),           // +40 最大生命（叠加到 statLifeMax2）
            StatEffect.LifeRegenMultiplier(1.5f), // 生命再生 *1.5 倍
            StatEffect.LifeRegen(2)           // 额外再 +2 lifeRegen（≈ 每秒 +1 血）
        ));
    }
}
*/

// ----------------------------------------------------------
// 设计说明 & 为什么把“属性修改”做成单独通道：
// ● Buff 与属性是两类不同的时序：
//   - Buff：不需要每帧都调用 AddBuff，使用“阈值刷新”在必要时顶时间即可（更省性能/网络）。
//   - 属性：必须每帧参与最终数值计算；因此用 ModPlayer 作为“累加槽”，所有道具把本帧改动写入累加器，
//     然后在 PostUpdateEquips 一次性应用，避免重复叠加、便于统一调试与平衡。
// ● 两条通道解耦后，你可以：
//   - 在同一件物品里同时声明 Buff 与属性；
//   - 任意组合条件（环境/时间/高度/生物群落），表现为“规则”。
// ● 和 Unity 里的接口思路对比：
//   - tModLoader 的生命周期由 Hook（如 HoldItem/UpdateAccessory/UpdateEquip）驱动；
//   - GlobalItem 充当“入口”，扫描实现了接口的物品并执行规则；
//   - 接口用于“声明能力”，GlobalItem/ModPlayer用于“承接生命周期与落地实现”。
// ● 兼容/扩展：
//   - 你可以继续给 StatEffect 增加更多工厂方法（比如伤害减免、跳跃次数、飞行时间等），
//     再在 BuffStatPlayer.PostUpdateEquips 里统一应用即可。
//   - 如果担心多件装备对同一属性的叠加顺序，可把“乘法”与“加法”分开字段并控制顺序（如先乘后加）。
// ----------------------------------------------------------
