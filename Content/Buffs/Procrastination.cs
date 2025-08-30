
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    public class Procrastination : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
