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
    // TODO: 增加贴图虚影特效
    public class DiamondSkin : JuexueItem
    {
        public override int QiCost => 100;
        public override int SpecialCooldownTicks => 60 * 60; // 60 秒

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("绝学·金刚不坏");
            // Tooltip.SetDefault("主动（100气）：5秒内最终伤害 -70%；60秒冷却。");
        }

        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            player.AddBuff(ModContent.BuffType<DiamondSkinBuff>(), 60 * 5);
            SoundEngine.PlaySound(SoundID.Item29, player.Center);
            return true;
        }
    }
}
