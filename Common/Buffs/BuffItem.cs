using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items.Weapons.Melee;
using WuDao.Content.Items.Accessories;
using WuDao.Content.Global.NPCs;
using Terraria.GameContent.UI.Minimap;

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
        public float MoveSpeedAdd = 0f;      // 加法（与 vanilla Player.moveSpeed 同维度）
        // public float MaxAccRunSpeed = 3f;    // accRunSpeed
        public float AccRunSpeedCandidate;  // 本帧声明的上限候选（取最大）
        public float AccRunSpeedAdd = 0f;
        public float MaxRunSpeedAdd = 0f;       // 叠加到 Player.maxRunSpeed
        public float RunAccelAdd = 0f;       // 叠加到 Player.runAcceleration
        public float RunSlowdownAdd = 0;
        public float JumpSpeedBoostAdd = 0f;      // 叠加到 Player.jumpSpeedBoost
        public int JumpHeightAdd = 0;
        public float JumpSpeed = 0;
        public int ExtraFall = 0;
        public int LifeRegenAdd = 0;       // 叠加到 Player.lifeRegen（约每秒回复 = lifeRegen/2 生命）
        public float LifeRegenMult = 1f; // 生命再生“倍率”
        public int MaxLifeAdd = 0;          // 叠加最大生命（作用于 statLifeMax2）
        public float MaxLifeMult = 1f;   // 百分比型最大生命加成（乘到 statLifeMax2）
        public float MeleeSizeMult = 1f;   // 近战武器尺寸加成（乘到 Player.meleeSize）
        public readonly HashSet<int> ImmuneBuffs = new();
        public float CritGenericAdd = 0f, CritMeleeAdd = 0f, CritRangedAdd = 0f, CritMagicAdd = 0f, CritSummonAdd = 0f;
        public bool FlagSlowFall, FlagNoFallDmg, FlagAutoJump, FlagJumpBoost;
        // 累加槽（放在已有字段旁）
        public int MaxManaAdd = 0;
        public float MaxManaMult = 1f;
        public int ManaRegenAdd = 0;
        public float ManaRegenMult = 1f;
        public float KnockbackMulti = 1f;
        public float EnduranceAdd = 0f;
        public int DefenseAdd = 0;          // 叠加防御
        public float DefenseMult = 1f;           // 防御倍率（在加法 DefenseAdd 之前乘）
        // BuffStatPlayer 字段
        public float AttackSpeed = 0f;   // 通用攻速加成 最终乘数
        public float AttackSpeedMelee = 0f;
        public float AttackSpeedRanged = 0f;
        public float AttackSpeedMagic = 0f;
        public float AttackSpeedSummon = 0f;
        public float AttackSpeedThrowing = 0f;
        public float DamageGeneric = 0f;
        public float MeleeDamageAdd = 0f;
        public float RangedDamageAdd = 0f;
        public float MagicDamageAdd = 0f;
        public float SummonDamageAdd = 0f;
        public float ThrowingDamageAdd = 0f;
        public bool FlagkbGlove = false;
        public bool MeleeScaleGlove = false;
        public bool AutoReuse = false;
        public bool FlagFireWalk = false;
        public bool FlagLavaImmune = false;
        public bool FlagNoKnockback = false;
        public bool FlagLongInvince = false;
        public int LavaMax = 0;
        public override void ResetEffects()
        {
            MoveSpeedAdd = 0f;
            MaxRunSpeedAdd = 0f; AccRunSpeedAdd = 0f; RunSlowdownAdd = 0; AccRunSpeedCandidate = 3f; RunAccelAdd = 0f;
            JumpSpeedBoostAdd = 0f; JumpSpeed = 0f; JumpHeightAdd = 0; ExtraFall = 0;
            LifeRegenAdd = 0; LifeRegenMult = 1f;
            DefenseAdd = 0; MaxLifeAdd = 0; MaxLifeMult = 1f; MeleeSizeMult = 1f;
            ImmuneBuffs.Clear();
            CritGenericAdd = CritMeleeAdd = CritRangedAdd = CritMagicAdd = CritSummonAdd = 0f;
            FlagSlowFall = FlagNoFallDmg = FlagAutoJump = FlagJumpBoost = false;
            MaxManaAdd = 0; MaxManaMult = 1f;
            ManaRegenAdd = 0; ManaRegenMult = 1f;
            AttackSpeed = 0f;
            AttackSpeedMelee = 0f;
            AttackSpeedRanged = 0f;
            AttackSpeedMagic = 0f;
            AttackSpeedSummon = 0f;
            AttackSpeedThrowing = 0f;
            AutoReuse = false;
            DamageGeneric = 0f;
            MeleeDamageAdd = 0f;
            RangedDamageAdd = 0f;
            MagicDamageAdd = 0f;
            SummonDamageAdd = 0f;
            ThrowingDamageAdd = 0f;
            DefenseMult = 1f;
            KnockbackMulti = 1f;
            FlagkbGlove = false;
            MeleeScaleGlove = false;
            EnduranceAdd = 0f;
            FlagFireWalk = false;
            FlagLavaImmune = false;
            LavaMax = 0;
            FlagNoKnockback = false;
            FlagLongInvince = false;
        }

        public override void UpdateEquips()
        {
            // 动态从“手持物品”拉取规则，并应用满足规则的加成到玩家属性中
            // 手持饰品则饰品效果不生效，饰品规则的应用放到GlobalItem中
            Item held = Player.HeldItem;
            if (held?.ModItem is IStatItemProvider statProvider && !held.accessory)
            {
                _tmpRules.Clear();
                statProvider.AddStatRules(Player, held, _tmpRules);

                // 仅筛选满足条件的规则
                foreach (var rule in _tmpRules)
                {
                    if (rule?.Condition == null || !rule.Condition(Player, held))
                        continue;

                    foreach (var eff in rule.Effects)
                        eff.Apply(Player, this); // 这一步会把相关属性增量写进对应字段
                }
            }
            // 速度系数 此加成效果会在后续应用到最大速度和冲刺速度上 放到PostUpdateRunSpeeds就太晚了
            // 先加后乘，增加一条乘积
            Player.moveSpeed += MoveSpeedAdd;
            // 取最大的鞋子设置的冲刺速度
            // if (AccRunSpeedCandidate > 3f)
            // Player.accRunSpeed = Math.Max(Player.accRunSpeed, AccRunSpeedCandidate);
            // 跳跃放这里，手持正常
            Player.jumpSpeedBoost += JumpSpeedBoostAdd;
            Player.extraFall += ExtraFall;

            Player.jumpSpeed += JumpSpeed;
            Player.jumpHeight += JumpHeightAdd;

            Player.lavaMax += LavaMax;
            if (FlagNoKnockback) Player.noKnockback = true;
            if (FlagJumpBoost) Player.jumpBoost = true;
            if (FlagAutoJump) Player.autoJump = true;
            if (FlagSlowFall) Player.slowFall = true;
            if (FlagFireWalk) Player.fireWalk = true;
            if (FlagLavaImmune) Player.lavaImmune = true;
            if (FlagLongInvince) Player.longInvince = true;
            // 应用本帧声明的免疫
            foreach (var buffId in ImmuneBuffs)
            {
                Player.buffImmune[buffId] = true;
                if (Player.HasBuff(buffId))
                    Player.ClearBuff(buffId);
            }
        }
        // BuffStatPlayer 内新增字段（缓存用，避免每帧分配）
        private static readonly List<StatRule> _tmpRules = new();
        // Use this to modify maxRunSpeed, accRunSpeed, runAcceleration, and similar variables before the player moves forwards/backwards.
        public override void PostUpdateRunSpeeds()
        {
            Player.runAcceleration *= 1f + RunAccelAdd;
            // 奔跑冲刺的最大速度 疾风雪靴就只提升了这个
            // +0.15 代表 +15% 移动的最大速度
            Player.maxRunSpeed *= 1f + MaxRunSpeedAdd;
            // 冲刺速度 与最大速度类似
            Player.accRunSpeed *= 1f + AccRunSpeedAdd;
            // 减速度
            Player.runSlowdown *= 1f + RunSlowdownAdd;

            if (AccRunSpeedCandidate > 3f)
                Player.accRunSpeed = Math.Max(Player.accRunSpeed, AccRunSpeedCandidate);
        }

        public override bool PreItemCheck()
        {
            Player.autoReuseGlove = AutoReuse;
            //+3 代表 +300%
            Player.GetDamage(DamageClass.Generic) += DamageGeneric;
            Player.GetDamage(DamageClass.Melee) += MeleeDamageAdd;
            Player.GetDamage(DamageClass.Ranged) += RangedDamageAdd;
            Player.GetDamage(DamageClass.Magic) += MagicDamageAdd;
            Player.GetDamage(DamageClass.Summon) += SummonDamageAdd;
            Player.GetDamage(DamageClass.Throwing) += ThrowingDamageAdd;
            // 攻速最终乘数 普通类的攻速加成会影响到所有类别的攻速
            Player.GetAttackSpeed(DamageClass.Generic) += AttackSpeed;
            Player.GetAttackSpeed(DamageClass.Melee) += AttackSpeedMelee + AttackSpeed;
            Player.GetAttackSpeed(DamageClass.Ranged) += AttackSpeedRanged + AttackSpeed;
            Player.GetAttackSpeed(DamageClass.Magic) += AttackSpeedMagic + AttackSpeed;
            Player.GetAttackSpeed(DamageClass.Summon) += AttackSpeedSummon + AttackSpeed;
            Player.GetAttackSpeed(DamageClass.Throwing) += AttackSpeedThrowing + AttackSpeed;
            // 挖矿速度怎么做？Player.pickSpeed
            // +30 代表 +30%
            Player.GetCritChance(DamageClass.Generic) += CritGenericAdd;
            Player.GetCritChance(DamageClass.Melee) += CritMeleeAdd;
            Player.GetCritChance(DamageClass.Ranged) += CritRangedAdd;
            Player.GetCritChance(DamageClass.Magic) += CritMagicAdd;
            Player.GetCritChance(DamageClass.Summon) += CritSummonAdd;
            // *2 代表 +100% 击退
            Player.GetKnockback(DamageClass.Melee) *= KnockbackMulti;
            // 耐力
            Player.endurance += EnduranceAdd;
            if (FlagkbGlove) Player.kbGlove = true;
            if (MeleeScaleGlove) Player.meleeScaleGlove = true;

            // +750 表示 +750 仇恨
            // Player.aggro
            // 召唤物击退怎么做？
            // Player.GetKnockback(DamageClass.Summon)
            return true;
        }
        public override void PostItemCheck()
        {
            // 最大生命：先百分比，再平移
            int newMaxLife = (int)Math.Round(Player.statLifeMax2 * MaxLifeMult) + MaxLifeAdd;
            if (newMaxLife < 1) newMaxLife = 1;
            Player.statLifeMax2 = newMaxLife;
            if (Player.statLife > Player.statLifeMax2)
                Player.statLife = Player.statLifeMax2;
            // 生命再生：先乘再加，便于和其他来源组合
            Player.lifeRegen = (int)Math.Round(Player.lifeRegen * LifeRegenMult);
            Player.lifeRegen += LifeRegenAdd;
            // 最大法力：先百分比，再平移
            Player.statManaMax2 = (int)Math.Round(Player.statManaMax2 * MaxManaMult) + MaxManaAdd;
            if (Player.statMana > Player.statManaMax2) Player.statMana = Player.statManaMax2;
            // 法力再生：先乘再加，便于和其他来源组合
            Player.manaRegen = (int)Math.Round(Player.manaRegen * ManaRegenMult) + ManaRegenAdd;
            // 一般都是先乘后加
            Player.statDefense *= DefenseMult;
            Player.statDefense += DefenseAdd;

            if (FlagNoFallDmg) Player.noFallDmg = true;
        }

    }
    // 一个“属性效果”的表示：
    public delegate void StatApplier(Player player, BuffStatPlayer acc);

    public readonly struct StatEffect
    {
        public readonly StatApplier Apply;
        public StatEffect(StatApplier apply) => Apply = apply ?? throw new ArgumentNullException(nameof(apply));

        // 常用工厂方法（可按需继续扩展）
        public static StatEffect MoveSpeed(float add = 0f) => new((p, acc) => { acc.MoveSpeedAdd += add; });
        // 直接赋值修改最大冲刺速度，少用为妙
        public static StatEffect AccRunSpeedSet(float cap) => new((p, acc) => acc.AccRunSpeedCandidate = Math.Max(acc.AccRunSpeedCandidate, cap));
        // 将最大冲刺速度增加
        public static StatEffect AccRunSpeed(float add) => new((p, acc) => acc.AccRunSpeedAdd += add);
        public static StatEffect MaxRunSpeed(float add) => new((p, acc) => acc.MaxRunSpeedAdd += add);
        public static StatEffect RunAcceleration(float add) => new((p, acc) => acc.RunAccelAdd += add);
        public static StatEffect RunSlowdown(float add) => new((p, acc) => acc.RunSlowdownAdd += add);
        public static StatEffect JumpSpeedBoost(float add) => new((p, acc) => acc.JumpSpeedBoostAdd += add);
        public static StatEffect JumpHeight(int add) => new((p, acc) => acc.JumpHeightAdd += add);
        public static StatEffect JumpSpeed(float add) => new((p, acc) => acc.JumpSpeed += add);
        public static StatEffect ExtraFall(int add) => new((p, acc) => acc.ExtraFall += add);
        public static StatEffect LifeRegen(int add) => new((p, acc) => acc.LifeRegenAdd += add);
        public static StatEffect LifeRegenMultiplier(float mult) => new((p, acc) => acc.LifeRegenMult *= mult);
        public static StatEffect LifeRegenPercent(float percent) => new((p, acc) => acc.LifeRegenMult *= 1f + percent);
        public static StatEffect Defense(int add) => new((p, acc) => acc.DefenseAdd += add);
        public static StatEffect MaxLife(int add) => new((p, acc) => acc.MaxLifeAdd += add);

        public static StatEffect MaxLifeMultiplier(float mult) => new((p, acc) => acc.MaxLifeMult *= mult);
        public static StatEffect MaxLifePercent(float percent) => new((p, acc) => acc.MaxLifeMult *= 1f + percent);
        public static StatEffect MeleeSizePercent(float percent) => new((p, acc) => acc.MeleeSizeMult *= (1f + percent));
        public static StatEffect ImmuneTo(params int[] buffIds) => new((p, acc) => { foreach (var id in buffIds) acc.ImmuneBuffs.Add(id); });
        public static StatEffect Crit(float add) => new((p, acc) => acc.CritGenericAdd += add);
        public static StatEffect MeleeCrit(float add) => new((p, acc) => acc.CritMeleeAdd += add);
        public static StatEffect RangedCrit(float add) => new((p, acc) => acc.CritRangedAdd += add);
        public static StatEffect MagicCrit(float add) => new((p, acc) => acc.CritMagicAdd += add);
        public static StatEffect SummonCrit(float add) => new((p, acc) => acc.CritSummonAdd += add);
        public static StatEffect SlowFall() => new((p, acc) => acc.FlagSlowFall = true);
        public static StatEffect NoFallDmg() => new((p, acc) => acc.FlagNoFallDmg = true);
        public static StatEffect ControlJump(bool enable) => new((p, acc) => acc.FlagAutoJump = enable);
        // 是否启用原版气球的提升跳跃能力，只能叠加1次 是直接赋值的 jumpHeight = 20;jumpSpeed = 6.51f;
        public static StatEffect JumpBoost() => new((p, acc) => acc.FlagJumpBoost = true);
        // 法力
        public static StatEffect MaxMana(int add) => new((p, acc) => acc.MaxManaAdd += add);
        public static StatEffect MaxManaMultiplier(float mult) => new((p, acc) => acc.MaxManaMult *= mult);
        public static StatEffect MaxManaPercent(float percent) => new((p, acc) => acc.MaxManaMult *= 1f + percent);
        public static StatEffect ManaRegen(int add) => new((p, acc) => acc.ManaRegenAdd += add);
        public static StatEffect ManaRegenMultiplier(float mult) => new((p, acc) => acc.ManaRegenMult *= mult);
        public static StatEffect ManaRegenPercent(float percent) => new((p, acc) => acc.ManaRegenMult *= 1f + percent);

        // 攻速 & 伤害（对所有类型）
        public static StatEffect DamageAdd(float percent) => new((p, acc) => acc.DamageGeneric += percent);
        public static StatEffect MeleeDamageAdd(float percent) => new((p, acc) => acc.MeleeDamageAdd += percent);
        public static StatEffect RangedDamageAdd(float percent) => new((p, acc) => acc.RangedDamageAdd += percent);
        public static StatEffect MagicDamageAdd(float percent) => new((p, acc) => acc.MagicDamageAdd += percent);
        public static StatEffect SummonDamageAdd(float percent) => new((p, acc) => acc.SummonDamageAdd += percent);
        public static StatEffect ThrowingDamageAdd(float percent) => new((p, acc) => acc.ThrowingDamageAdd += percent);
        public static StatEffect AttackSpeedAdd(float percent) => new((p, acc) => acc.AttackSpeed += percent);
        public static StatEffect MeleeAttackSpeedAdd(float percent) => new((p, acc) => acc.AttackSpeedMelee += percent);
        public static StatEffect RangedAttackSpeedAdd(float percent) => new((p, acc) => acc.AttackSpeedRanged += percent);
        public static StatEffect MagicAttackSpeedAdd(float percent) => new((p, acc) => acc.AttackSpeedMagic += percent);
        public static StatEffect SummonAttackSpeedAdd(float percent) => new((p, acc) => acc.AttackSpeedSummon += percent);
        public static StatEffect ThrowingAttackSpeedAdd(float percent) => new((p, acc) => acc.AttackSpeedThrowing += percent);
        public static StatEffect KnockbackMulti(float percent) => new((p, acc) => acc.KnockbackMulti += percent);
        public static StatEffect KbGlove() => new((p, acc) => acc.FlagkbGlove = true);
        public static StatEffect MeleeScaleGlove() => new((p, acc) => acc.MeleeScaleGlove = true);
        public static StatEffect EnduranceAdd(float add) => new((p, acc) => acc.EnduranceAdd += add);
        // 自动重复攻击
        public static StatEffect AutoReuse() => new((p, acc) => acc.AutoReuse = true);

        // 防御倍率
        public static StatEffect DefenseMultiplier(float mult) => new((p, acc) => acc.DefenseMult *= mult);
        public static StatEffect DefensePercent(float percent) => new((p, acc) => acc.DefenseMult *= 1f + percent);

        public static StatEffect FireWalk() => new((p, acc) => acc.FlagFireWalk = true);
        public static StatEffect LavaImmune() => new((p, acc) => acc.FlagLavaImmune = true);
        public static StatEffect LavaMaxAdd(int add) => new((p, acc) => acc.LavaMax += add);
        public static StatEffect NoKnockback() => new((p, acc) => acc.FlagNoKnockback = true);
        public static StatEffect LongInvince() => new((p, acc) => acc.FlagLongInvince = true);
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
        // 所有 BuffItem 的规则将搜集到一起，最后由 GlobalItem 去执行
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
        // 应用饰品的近战尺寸加成
        public override void ModifyItemScale(Item item, Player player, ref float scale)
        {
            if (item.CountsAsClass(DamageClass.Melee))
            {
                var acc = player.GetModPlayer<BuffStatPlayer>();
                scale *= acc.MeleeSizeMult;   // 你框架里定义的近战武器尺寸倍率，默认 1f
            }
        }
        // ① 收集饰品规则，记录标志位 饰品已装备
        public override void UpdateEquip(Item item, Player player)
        {
            if (item.accessory)
            {
                TryApplyFromItem(player, item); // 你已有的收集规则调用
                if (item.type == ModContent.ItemType<DevolutionCharm>())
                {
                    player.GetModPlayer<DevolutionPlayer>().HasDevolutionAura = true;
                }
                else if (item.type == ModContent.ItemType<WrathLotus>())
                {
                    player.GetModPlayer<WrathLotusPlayer>().hasLotus = true;
                }
            }
        }
        // 收集装备或饰品的规则
        private static void TryApplyFromItem(Player player, Item item)
        {
            if (item?.ModItem == null) return;
            // 属性规则
            if (item.ModItem is IStatItemProvider statProvider)
            {
                _statRules.Clear();
                statProvider.AddStatRules(player, item, _statRules);
                ApplyStatRules(player, item, _statRules);
            }
            // Buff 规则
            if (item.ModItem is IBuffItemProvider buffProvider)
            {
                _buffRules.Clear();
                buffProvider.AddBuffRules(player, item, _buffRules);
                ApplyBuffRulesSmart(player, item, _buffRules);
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
