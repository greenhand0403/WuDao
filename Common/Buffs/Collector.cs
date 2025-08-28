
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Common.Buffs
{
    public class Collector : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
