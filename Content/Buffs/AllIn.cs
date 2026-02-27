using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 孤注一掷：空背包与空饰品栏越多，伤害/移速/暴击越高，但减伤越低
    public class AllIn : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }
    }
}