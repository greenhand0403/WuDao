using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace WuDao.Common
{
    public class BladeTrailConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("全局刀光")]
        [DefaultValue(true)]
        // [Label("启用全局顶点刀光效果")]
        public bool EnableVertexBladeTrail { get; set; } = true;

        [Header("默认集合")]
        [DefaultValue(true)]
        // [Label("包含默认白名单（BladeTrailSet）")]
        public bool IncludeDefaultBladeTrailSet { get; set; } = true;

        [DefaultValue(true)]
        // [Label("包含相位剑/光剑集合（可选）")]
        public bool IncludePhaseblades { get; set; } = true;

        [Header("自定义白名单")]
        // [Label("武器白名单（可增删）")]
        public List<ItemDefinition> Whitelist { get; set; } = new();

        public override void OnLoaded()
        {
            // 直接使用当前实例，避免在此时序里再去 GetInstance 造成 null
            BladeTrailRuntime.ApplyFromConfig(this);
        }

        public override void OnChanged()
        {
            BladeTrailRuntime.ApplyFromConfig(this);
        }
    }
}
