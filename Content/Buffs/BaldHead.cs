using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 光头buff
    public class BaldHead : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;        // 不是减益
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = false;    // 允许存档
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
