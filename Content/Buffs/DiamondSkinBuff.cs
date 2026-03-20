using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 金刚不坏绝学给予的buff 耐力增加70%（endurance +0.7）
    public class DiamondSkinBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.endurance += 0.70f;
        }
    }
}
