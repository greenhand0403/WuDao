using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace WuDao.Content.Config
{
    public class JuexueServerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        public override void OnLoaded()
        {
            JuexueRuntime.ApplyFromConfig(this);
        }

        public override void OnChanged()
        {
            JuexueRuntime.ApplyFromConfig(this);
        }
    }
}