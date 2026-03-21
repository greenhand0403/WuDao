using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Config
{
    // 隐藏敌怪物块机制的配置
    public static class InvisibleTileRuntime
    {
        // 默认关闭，直到配置应用成功
        public static bool Enabled { get; private set; } = false;
        public static HashSet<int> InvisibleTileIDSet = new();

        public static void Clear()
        {
            Enabled = false;
            InvisibleTileIDSet.Clear();
        }

        public static void ApplyFromConfig(InvisibleEnemiesConfig cfg)
        {
            Clear();

            if (cfg == null)
                return;

            Enabled = cfg.InvisibleEnemies;

            if (!Enabled || cfg.InvisibleTileIDs == null)
                return;

            foreach (var def in cfg.InvisibleTileIDs)
            {
                if (def?.Type >= 0 && def.Type < TileID.Count)
                {
                    InvisibleTileIDSet.Add(def.Type);
                }
            }
        }

        /// <summary>在“非配置回调”的时机尝试重建（拿不到就保持关闭，不抛异常）</summary>
        public static void TryRebuildFromConfig()
        {
            InvisibleEnemiesConfig cfg = null;
            try
            {
                cfg = ModContent.GetInstance<InvisibleEnemiesConfig>();
            }
            catch
            {
            }

            ApplyFromConfig(cfg);
        }

        public static bool IsTileInvisible(ushort type)
        {
            if (!Enabled)
                return false;

            return InvisibleTileIDSet.Contains(type);
        }
    }
}