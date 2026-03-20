using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace WuDao.Content.Config
{
    // 盔甲五行系统
    public class ArmorPrefixConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        // ✅ 配置在菜单里的名字（可本地化）
        public override LocalizedText DisplayName =>
            Language.GetText("Mods.WuDao.Configs.ArmorPrefixConfig.DisplayName");

        public enum ArmorPrefixMode
        {
            Disabled,
            OreArmorOnly,
            AllArmor
        }

        [Header("$Mods.WuDao.Configs.ArmorPrefixConfig.Headers.Main")]
        [DefaultValue(ArmorPrefixMode.OreArmorOnly)]
        public ArmorPrefixMode PrefixMode { get; set; } = ArmorPrefixMode.OreArmorOnly;
    }
}