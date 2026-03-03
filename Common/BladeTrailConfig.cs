using System.Collections.Generic;
using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace WuDao.Common
{
    public class BladeTrailConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        // ✅ 配置标题（显示在模组配置列表里）
        public override LocalizedText DisplayName =>
            Language.GetText("Mods.WuDao.Configs.BladeTrailConfig.DisplayName");
        // ✅ 全局刀光配置
        [Header("$Mods.WuDao.Configs.BladeTrailConfig.Headers.GlobalBladeTrail")]
        [LabelKey("$Mods.WuDao.Configs.BladeTrailConfig.EnableVertexBladeTrail.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailConfig.EnableVertexBladeTrail.Tooltip")]
        [DefaultValue(true)]
        public bool EnableVertexBladeTrail { get; set; } = true;
        // ✅ 默认白名单配置
        [Header("$Mods.WuDao.Configs.BladeTrailConfig.Headers.DefaultSets")]
        [LabelKey("$Mods.WuDao.Configs.BladeTrailConfig.IncludeDefaultBladeTrailSet.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailConfig.IncludeDefaultBladeTrailSet.Tooltip")]
        [DefaultValue(true)]
        public bool IncludeDefaultBladeTrailSet { get; set; } = true;
        // ✅ 包含光剑
        [LabelKey("$Mods.WuDao.Configs.BladeTrailConfig.IncludePhaseblades.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailConfig.IncludePhaseblades.Tooltip")]
        [DefaultValue(true)]
        public bool IncludePhaseblades { get; set; } = true;
        // ✅ 白名单配置
        [Header("$Mods.WuDao.Configs.BladeTrailConfig.Headers.CustomWhitelist")]
        [LabelKey("$Mods.WuDao.Configs.BladeTrailConfig.Whitelist.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailConfig.Whitelist.Tooltip")]
        public List<ItemDefinition> Whitelist { get; set; } = new();

        public override void OnLoaded() => BladeTrailRuntime.ApplyFromConfig(this);
        public override void OnChanged() => BladeTrailRuntime.ApplyFromConfig(this);
    }
}