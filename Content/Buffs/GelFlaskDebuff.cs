using Terraria;
using Terraria.ModLoader;
namespace WuDao.Content.Buffs
{
    // 凝胶灌注减益buff 作用于敌怪
    class GelFlaskDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }
    }
}
