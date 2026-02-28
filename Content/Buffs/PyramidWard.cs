
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 金字塔守护buff
    public class PyramidWard : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = true;
        }
    }
}
