
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 糖果敏捷buff
    public class SweetAgile : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
