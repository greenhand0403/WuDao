
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Common.Buffs {
    public class SacrificialBranding : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
