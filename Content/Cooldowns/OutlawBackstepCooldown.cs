using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace WuDao.Content.Cooldowns
{
    // 霰弹枪后跳冷却
    public class OutlawBackstepCooldown : ModBuff
    {
        public override string Texture => $"Terraria/Images/Buff_1";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("后跳冷却");
            // Description.SetDefault("后跳正在冷却中");
            Main.debuff[Type] = true;               // 用 debuff 风格显示
            Main.pvpBuff[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;   // 显示倒计时
            Main.buffNoSave[Type] = true;           // 不存档
        }
    }
}
