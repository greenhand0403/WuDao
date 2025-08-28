
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Common.Buffs
{
    public class PyramidWard : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("金字塔守护");
            // Description.SetDefault("静止站立在实心块上时获得强力减伤");
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
