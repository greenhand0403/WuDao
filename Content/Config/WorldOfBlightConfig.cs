using System.ComponentModel;
using Microsoft.Xna.Framework; // MathHelper
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace WuDao.Content.Config
{
    // TODO: 翻译世界之毒配置
    // ================================
    // 1) 配置：在模组配置菜单可调 人间之毒 人生病了产生白细胞，地球生病了产生生物，活着的都是勇者，每时每刻都在承受世界之毒。随游戏进度增强。
    // ================================
    public class WorldOfBlightConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide; // 影响玩法，放到服务端配置


        [Label("启用世界之毒全局效果")]
        [DefaultValue(true)]
        public bool Enabled { get; set; }


        [Label("每个进度的伤害减少(%)")]
        [Tooltip("每达成一个进度，玩家造成的所有伤害按百分比减少。范围 0%~10%。默认 4%。")]
        [Range(0f, 0.10f)]
        [Increment(0.01f)]
        [DefaultValue(0.04f)]
        public float DamagePenaltyPerStep { get; set; }


        [Label("每个进度的生命再生减少(每秒)")]
        [Tooltip("每达成一个进度，玩家生命再生每秒减少的数值。范围 0~20。默认 4。提示：Terraria内部以每秒*2结算，代码中会自动换算。")]
        [Range(0, 20)]
        [DefaultValue(4)]
        public int LifeRegenPenaltyPerStep { get; set; }


        [Label("每个进度的防御减少")]
        [Tooltip("每达成一个进度，玩家防御减少的数值。范围 0~30。默认 4。")]
        [Range(0, 30)]
        [DefaultValue(4)]
        public int DefensePenaltyPerStep { get; set; }


        [Label("伤害最小倍率下限")]
        [Tooltip("为避免伤害被削成0，这里设置一个下限。例如 0.1 表示最低保留10%伤害。设置为0则不设下限。")]
        [Range(0f, 1f)]
        [Increment(0.05f)]
        [DefaultValue(0f)]
        public float DamageFloorMultiplier { get; set; }


        [Label("防止本DEBUFF把生命再生降为负数")]
        [Tooltip("勾选后：世界之毒在削减生命再生时，最多把正向生命再生削到 0，不会由本效果导致负数。默认启用。")]
        [DefaultValue(true)]
        public bool ClampLifeRegenNonNegative { get; set; }
    }

    // =====================================
    // 2) ModPlayer：按进度应用全局削弱
    // =====================================
    public class WorldOfBlightPlayer : ModPlayer
    {
        private int _steps; // 当前达成的进度数（0~9）
        private float _damagePenaltyPerStep;
        private int _lifeRegenPenaltyPerStep;
        private int _defensePenaltyPerStep;
        private float _damageFloor;

        public override void ResetEffects()
        {
            // 每tick刷新一次配置与进度
            var cfg = ModContent.GetInstance<WorldOfBlightConfig>();
            if (!cfg.Enabled)
            {
                _steps = 0;
                return;
            }

            _steps = CountProgressSteps();
            _damagePenaltyPerStep = MathHelper.Clamp(cfg.DamagePenaltyPerStep, 0f, 0.10f);
            _lifeRegenPenaltyPerStep = Utils.Clamp(cfg.LifeRegenPenaltyPerStep, 0, 20);
            _defensePenaltyPerStep = Utils.Clamp(cfg.DefensePenaltyPerStep, 0, 30);
            _damageFloor = MathHelper.Clamp(cfg.DamageFloorMultiplier, 0f, 1f);
        }

        // 伤害：统一从 Generic 入手，影响近战/远程/魔法/召唤/投掷的最终伤害修正
        public override void PostUpdateEquips()
        {
            if (_steps <= 0) return;

            float totalPenalty = _steps * _damagePenaltyPerStep; // 例如 9 * 0.04 = 0.36
            float mul = 1f - totalPenalty;
            if (_damageFloor > 0f)
                mul = MathHelper.Max(mul, _damageFloor);

            // 对所有伤害类型施加倍率（Generic 会传导到所有具体伤害类型）
            Player.GetDamage(DamageClass.Generic) *= mul;
        }

        // 防御：直接减在最终防御上
        public override void PostUpdate()
        {
            if (_steps <= 0) return;
            int totalDefLoss = _steps * _defensePenaltyPerStep;
            Player.statDefense -= totalDefLoss;
            if (Player.statDefense < 0)
                Player.statDefense *= 0; // 防止出现负防御（可按需删除）
        }

        // 生命再生：内部以每秒*2处理，因此要*2
        public override void UpdateBadLifeRegen()
        {
            if (_steps <= 0) return;
            int totalRegenLossPerSecond = _steps * _lifeRegenPenaltyPerStep;
            int penaltyTicks = totalRegenLossPerSecond * 2; // 内部以每秒*2


            var cfg = ModContent.GetInstance<WorldOfBlightConfig>();
            if (cfg.Enabled && cfg.ClampLifeRegenNonNegative)
            {
                // 只削减“正向”的生命再生，不会把它削成负数
                int current = Player.lifeRegen;
                if (current > 0)
                {
                    int reduction = System.Math.Min(penaltyTicks, current);
                    Player.lifeRegen = current - reduction; // 最多降到 0
                }
                // 如果当前已经<=0（来自流血、狱炎等），不在这里进一步拉低，避免“本模组效果”导致更负
            }
            else
            {
                Player.lifeRegen -= penaltyTicks;
            }
        }

        // 计算进度：
        // 1. 腐/猩：世界吞噬者 或 克苏鲁之脑（满足其一）
        // 2. 骷髅王
        // 3. 血肉墙
        // 4. 任意一个机械BOSS
        // 5. 两个机械BOSS
        // 6. 三个机械BOSS
        // 7. 世花（世纪之花）
        // 8. 教徒（远古异教徒）
        // 9. 月总（月亮领主）
        private static int CountProgressSteps()
        {
            int steps = 0;

            // (1) EoW/BoC 任意
            if (NPC.downedBoss2) steps++;
            // (2) Skeletron
            if (NPC.downedBoss3) steps++;
            // (3) WoF
            if (Main.hardMode) steps++; // 或 NPC.downedBoss4

            // (4~6) 机械三王数量
            int mechCount = 0;
            if (NPC.downedMechBoss1) mechCount++;
            if (NPC.downedMechBoss2) mechCount++;
            if (NPC.downedMechBoss3) mechCount++;
            if (mechCount >= 1) steps++;
            if (mechCount >= 2) steps++;
            if (mechCount >= 3) steps++;

            // (7) 世花
            if (NPC.downedPlantBoss) steps++;
            // (8) 教徒
            if (NPC.downedAncientCultist) steps++;
            // (9) 月总
            if (NPC.downedMoonlord) steps++;

            // 保险：最多 9 步
            if (steps > 9) steps = 9;
            return steps;
        }
    }
}
