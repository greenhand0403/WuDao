using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    public class ShortInvulnBuff : ModBuff
    {
        // TODO: 部分buff贴图未更换
        public override string Texture => "Terraria/Images/Buff_2";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.immune = true;
            // player.immuneTime = 5; // 每帧刷新
        }
    }
}
