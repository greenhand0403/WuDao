
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 枯萎心灵buff
    public class WitheredMind : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
