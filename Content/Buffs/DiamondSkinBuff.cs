using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 最终伤害 -70%（= endurance +0.7）
    // TODO: 增加金刚不坏的贴图
    public class DiamondSkinBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_1";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.endurance += 0.70f;
        }
    }
}
