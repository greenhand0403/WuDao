using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // TODO: 乾坤大挪移的“无敌帧”Buff：给予更高的 i-frames（并不完全覆盖所有来源，但足够演示）
    public class ShortInvulnBuff : ModBuff
    {
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
            player.immuneTime = 60; // 每帧刷新
        }
    }
}
