
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 天人五衰buff
    public class FiveDecay : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
