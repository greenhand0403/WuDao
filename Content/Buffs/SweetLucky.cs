
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 糖果幸运buff
    public class SweetLucky : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
