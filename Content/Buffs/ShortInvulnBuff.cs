using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 短暂无敌buff
    public class ShortInvulnBuff : ModBuff
    {
        // 释放乾坤大挪移时给玩家的临时无敌buff
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
