
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 幻想破灭debuff
    public class BrokenDreams : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
        }
    }
}
