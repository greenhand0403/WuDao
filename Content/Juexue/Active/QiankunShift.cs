using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Content.Buffs;
using WuDao.Common;

namespace WuDao.Content.Juexue.Active
{
    // TODO: 增加弧线的人物运动残影，增加太极图虚影
    public class QiankunShift : JuexueItem
    {
        public override int QiCost => 100;
        public override int SpecialCooldownTicks => 60 * 60; // 60 秒

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("绝学·乾坤大挪移");
            // Tooltip.SetDefault("主动（100气）：瞬移到光标位置并获得1秒无敌；60秒内不能再次使用。");
        }

        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            Vector2 mouse = Main.MouseWorld;
            player.Teleport(mouse, 1, 0);
            player.AddBuff(ModContent.BuffType<ShortInvulnBuff>(), 60);
            SoundEngine.PlaySound(SoundID.Item6, mouse);
            return true;
        }
    }
}
