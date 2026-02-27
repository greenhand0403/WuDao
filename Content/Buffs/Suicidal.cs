
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 寻死buff
    public class Suicidal : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
