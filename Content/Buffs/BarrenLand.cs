using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 贫瘠之地：身上 Buff 越多，生命流失越快
    public class BarrenLand : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }
    }
}