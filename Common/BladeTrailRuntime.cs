using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WuDao.Common
{
    public static class BladeTrailRuntime
    {
        // 默认关闭，直到配置应用成功
        public static bool Enabled { get; private set; } = false;

        public static readonly HashSet<int> AllowedItemIDs = new();

        /// <summary>由配置实例应用（推荐：在 ModConfig.OnLoaded / OnChanged 里调用）</summary>
        public static void ApplyFromConfig(BladeTrailConfig cfg)
        {
            if (cfg == null)
            {
                // 配置不可用时，保持关闭状态，避免 NRE
                Enabled = false;
                AllowedItemIDs.Clear();
                return;
            }

            Enabled = cfg.EnableVertexBladeTrail;
            AllowedItemIDs.Clear();

            if (!Enabled)
                return;

            // 1) 默认集合
            if (cfg.IncludeDefaultBladeTrailSet)
            {
                foreach (var id in ItemSets.BladeTrailSet)
                    AllowedItemIDs.Add(id);
            }

            // 2) 可选集合（如果你有）
            if (cfg.IncludePhaseblades && ItemSets.PhasebladeSet is not null)
            {
                foreach (var id in ItemSets.PhasebladeSet)
                    AllowedItemIDs.Add(id);
            }

            // 3) 用户白名单（覆盖/补充）
            foreach (var def in cfg.Whitelist)
            {
                if (def?.Type > 0)
                    AllowedItemIDs.Add(def.Type);
            }
        }

        /// <summary>在“非配置回调”的时机尝试重建（拿不到就保持关闭，不抛异常）</summary>
        public static void TryRebuildFromConfig()
        {
            BladeTrailConfig cfg = null;
            try
            {
                cfg = ModContent.GetInstance<BladeTrailConfig>();
            }
            catch
            {
                // 某些早期生命周期可能会抛，忽略即可
            }

            ApplyFromConfig(cfg);
        }

        public static bool IsAllowed(Item item)
        {
            if (!Enabled || item is null)
                return false;

            if (!item.DamageType.CountsAsClass(DamageClass.Melee) || item.noMelee)
                return false;

            return AllowedItemIDs.Contains(item.type);
        }
    }
}
