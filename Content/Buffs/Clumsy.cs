
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 笨拙debuff
    public class Clumsy : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
