using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WuDao.Content.Items.Weapons.Melee;

namespace WuDao.Content.Buffs
{
    public class NormalScissorsDashBuff : ModBuff
    {
        public override string Texture => $"WuDao/Content/Items/Weapons/Melee/NormalScissors";
        public override void SetStaticDefaults()
        {
            // 主动显示、可见倒计时
            Main.debuff[Type] = true;          // 视为debuff（只影响显示分类）
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = true;      // 不存档
            Main.buffNoTimeDisplay[Type] = false; // 显示时间
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 用 Buff 的剩余时间反向同步到 ModPlayer，让逻辑仍能用 dashCooldown
            var mp = player.GetModPlayer<NormalScissorsPlayer>();
            mp.dashCooldown = player.buffTime[buffIndex];
        }
    }

}