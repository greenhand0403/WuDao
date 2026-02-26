using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WuDao.Content.Buffs
{
    // 重负: 背包越满，生命流失越快
    public class Encumbered : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }
    }
}