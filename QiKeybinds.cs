using Terraria.ModLoader;
using Terraria;

namespace WuDao
{
    // 绝学系统 气力键
    public class QiKeybinds : ModSystem
    {
        // 一个按键同时负责“按住蓄力/松开释放”（龟派气功）
        public static ModKeybind CastSkillKey { get; private set; } = null!;

        public override void Load()
        {
            CastSkillKey = KeybindLoader.RegisterKeybind(Mod, "Active JueXue Skill", "V");
        }

        public override void Unload()
        {
            CastSkillKey = null!;
        }
    }
}
