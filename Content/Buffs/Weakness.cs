
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 虚弱buff
    public class Weakness : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
