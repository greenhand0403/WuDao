using Terraria.ModLoader.Config;

namespace WuDao.Content.Config
{
    public class ArmorPrefixConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        // [Label("盔甲前缀适用范围")]
        [OptionStrings(new string[] { "禁用", "仅矿物盔甲", "所有盔甲" })]
        public string PrefixMode = "仅矿物盔甲";
    }
}
