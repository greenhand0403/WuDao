
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 金钱至上buff
    public class Capitalism : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
