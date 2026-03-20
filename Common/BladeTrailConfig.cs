using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace WuDao.Common
{
    public class BladeTrailConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public override LocalizedText DisplayName =>
            Language.GetText("Mods.WuDao.Configs.BladeTrailConfig.DisplayName");

        [LabelKey("$Mods.WuDao.Configs.BladeTrailConfig.GlobalBladeTrail.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailConfig.GlobalBladeTrail.Tooltip")]
        [DefaultValue(true)]
        public bool GlobalBladeTrail { get; set; } = true;

        public override void OnLoaded() => BladeTrailRuntime.ApplyVisualConfig(this);
        public override void OnChanged() => BladeTrailRuntime.ApplyVisualConfig(this);
    }
}