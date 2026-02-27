
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 生命诅咒buff
    public class LifeCurse : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
