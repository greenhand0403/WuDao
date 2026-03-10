using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace WuDao.Content.Config
{
    public static class InvisibleTileRuntime
    {
        // 默认关闭，直到配置应用成功
        public static bool Enabled { get; private set; } = false;
        public static HashSet<int> InvisibleTileIDSet = new();

        public static void ApplyFromConfig(WudaoConfig cfg)
        {
            if (cfg == null) return;
            Enabled = cfg.InvisibleEnemies;

            InvisibleTileIDSet.Clear();
            if (!Enabled || cfg.InvisibleTileIDs == null) return;

            foreach (var def in cfg.InvisibleTileIDs)
            {
                if (def?.Type >= 0 && def.Type <= TileID.Count)
                {
                    if (!InvisibleTileIDSet.Contains(def.Type))
                        InvisibleTileIDSet.Add(def.Type);
                }
            }
        }
        /// <summary>在“非配置回调”的时机尝试重建（拿不到就保持关闭，不抛异常）</summary>
        public static void TryRebuildFromConfig()
        {
            WudaoConfig cfg = null;
            try
            {
                cfg = ModContent.GetInstance<WudaoConfig>();
            }
            catch
            {
                // 某些早期生命周期可能会抛，忽略即可
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
    /// <summary>
    /// 默认开启 juexue 系统、关闭敌怪和陷阱隐身、允许幽灵护目镜看见回声块
    /// </summary>
    public class WudaoConfig : ModConfig
    {
        // 多人联机要统一就用 ServerSide；只影响本地表现就用 ClientSide
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        public bool EnableJueXueSystem;

        /// <summary>敌怪和陷阱隐身</summary>
        [DefaultValue(false)]
        public bool InvisibleEnemies;

        [DefaultValue(false)]
        /// <summary>忽略幽灵护目镜</summary>
        public bool IgnoreSpectreGoggles;
        // ✅ 白名单配置
        [LabelKey("$Mods.WuDao.Configs.WudaoConfig.InvisibleEnemies.Whitelist")]
        public List<TileDefinition> InvisibleTileIDs { get; set; } = new () {
            new TileDefinition(TileID.Traps),//陷阱
            new TileDefinition(TileID.Spikes),//尖刺
            new TileDefinition(TileID.WoodenSpikes),//木刺
            new TileDefinition(TileID.Meteorite),//陨石
            new TileDefinition(TileID.Hellstone),//狱石
            new TileDefinition(TileID.WaterCandle),//水蜡烛
            new TileDefinition(TileID.Cobweb),//蜘蛛网
            new TileDefinition(TileID.GeyserTrap),//喷泉
            new TileDefinition(TileID.LandMine),//地雷
            new TileDefinition(TileID.Boulder),//巨石
            new TileDefinition(TileID.PressurePlates),//压力板
            new TileDefinition(TileID.Detonator),//引爆器
            new TileDefinition(TileID.Explosives),//炸药
            new TileDefinition(TileID.BreakableIce),//薄冰
            new TileDefinition(TileID.TNTBarrel),//TNT
            new TileDefinition(TileID.LifeCrystalBoulder),//生命水晶巨石
            new TileDefinition(TileID.BeeHive),//蜂巢
            new TileDefinition(TileID.AntlionLarva),//蚁狮卵
            new TileDefinition(TileID.Larva),//蜂王卵
            new TileDefinition(TileID.PlanteraBulb),//世纪之花灯泡
            new TileDefinition(TileID.RollingCactus),//仙人掌球
            new TileDefinition(TileID.LifeFruit),//生命果
            new TileDefinition(TileID.Heart),//生命水晶
        };
        public override void OnLoaded() => InvisibleTileRuntime.ApplyFromConfig(this);

        public override void OnChanged() => InvisibleTileRuntime.ApplyFromConfig(this);
    }
}