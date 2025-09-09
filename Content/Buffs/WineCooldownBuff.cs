using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    public class WineCooldownBuff : ModBuff
    {
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Burning}";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("永生之酒冷却");
            // Tooltip.SetDefault("你暂时无法再次饮用永生之酒");
            Main.debuff[Type] = true;      // 作为“负面”显示（仅用于提示，防止被部分清 Buff 手段移除）
            Main.buffNoTimeDisplay[Type] = false;
            Main.pvpBuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }
}
