using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Common
{
    public static class BladeTrailRuntime
    {
        // 视觉开关：只影响绘制
        public static bool VisualEnabled { get; private set; } = true;

        // 服务器规则：影响真实判定
        public static bool ServerRuleEnabled { get; private set; } = false;

        public static readonly HashSet<int> AllowedItemIDs = new();

        // 多人时用于保底：规则未就绪则不要启用替代判定
        public static bool ServerRuleReady { get; private set; } = false;

        public static void ApplyVisualConfig(BladeTrailConfig cfg)
        {
            if (cfg == null)
            {
                VisualEnabled = true;
                return;
            }

            VisualEnabled = cfg.GlobalBladeTrail;
        }

        public static void ApplyServerConfig(BladeTrailServerConfig cfg)
        {
            if (cfg == null)
            {
                ServerRuleEnabled = false;
                AllowedItemIDs.Clear();
                ServerRuleReady = Main.netMode == Terraria.ID.NetmodeID.SinglePlayer;
                return;
            }

            ServerRuleEnabled = cfg.EnableWhitelistBladeTrail;
            AllowedItemIDs.Clear();

            if (!ServerRuleEnabled)
            {
                ServerRuleReady = true;
                return;
            }

            // 1) 如果勾选了包含默认集合
            if (cfg.IncludeDefaultBladeTrailSet)
            {
                foreach (var id in ItemSets.BladeTrailSet)
                    AllowedItemIDs.Add(id);
            }

            // 2) 如果勾选了包含光剑集合
            if (cfg.IncludePhaseblades && ItemSets.PhasebladeSet is not null)
            {
                foreach (var id in ItemSets.PhasebladeSet)
                    AllowedItemIDs.Add(id);
            }

            // 3) 如果勾选了包含用户白名单
            if (cfg.EnableWhitelistBladeTrail && cfg.Whitelist is not null)
            {
                foreach (var def in cfg.Whitelist)
                {
                    if (def?.Type > 0)
                        AllowedItemIDs.Add(def.Type);
                }
            }

            ServerRuleReady = true;
        }

        public static void TryRebuildFromServerConfig()
        {
            BladeTrailServerConfig cfg = null;
            try
            {
                cfg = ModContent.GetInstance<BladeTrailServerConfig>();
            }
            catch
            {
            }

            ApplyServerConfig(cfg);
        }

        public static void ClearServerRule()
        {
            ServerRuleEnabled = false;
            AllowedItemIDs.Clear();
            ServerRuleReady = Main.netMode == Terraria.ID.NetmodeID.SinglePlayer;
        }

        public static bool IsAllowedByServerRule(Item item)
        {
            if (item is null)
                return false;

            if (!ServerRuleReady)
                return false;

            if (!ServerRuleEnabled)
                return false;

            if (!item.DamageType.CountsAsClass(DamageClass.Melee) || item.noMelee)
                return false;

            return AllowedItemIDs.Contains(item.type);
        }

        public static bool ShouldDrawVisual(Item item)
        {
            if (!VisualEnabled || item is null)
                return false;

            return item.DamageType.CountsAsClass(DamageClass.Melee) && !item.noMelee;
        }
    }
}