
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 收集者buff
    public class Collector : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
        }
    }
}
