
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 拖延症buff
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
