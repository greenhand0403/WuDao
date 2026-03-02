using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 永生之酒冷却buff
    public class WineCooldownBuff : ModBuff
    {
        public override string Texture => $"WuDao/Content/Items/EverlastingWine";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;// 作为“负面”显示（仅用于提示，防止被部分清 Buff 手段移除）
            Main.buffNoTimeDisplay[Type] = false;
            Main.pvpBuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }
}
