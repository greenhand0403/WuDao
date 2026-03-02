using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace WuDao.Content.Config
{
    /// <summary>
    /// 默认开启 juexue 系统
    /// </summary>
    public class WudaoConfig : ModConfig
    {
        // 多人联机要统一就用 ServerSide；只影响本地表现就用 ClientSide
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue(true)]
        public bool EnableJueXueSystem;
    }
}