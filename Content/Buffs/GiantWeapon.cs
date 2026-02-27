
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 武器巨大化buff
    public class GiantWeapon : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
