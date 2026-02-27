
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    /// <summary>
    /// 急性子buff
    /// </summary>
    public class Impatient : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
