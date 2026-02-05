using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Content.Buffs;
using WuDao.Common;
using Microsoft.Xna.Framework;
using System;

namespace WuDao.Content.Juexue.Active
{
    // 金刚不坏 绝学
    public class DiamondSkin : JuexueItem
    {
        public override int QiCost => 90;
        public override int SpecialCooldownTicks => 60 * 60; // 60 秒
        public const int DiamondSkinFrameIndex = 2;
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            Helpers.BossProgressBonus progressBonus = Helpers.BossProgressPower.Get(player);
            int time = (int)(60 * 5 * progressBonus.DamageMult);
            player.AddBuff(ModContent.BuffType<DiamondSkinBuff>(), time);
            SoundEngine.PlaySound(SoundID.Item29, player.Center);
            if (!Main.dedServ)
            {
                // 冷却图标
                qi.TriggerJuexueCooldownIcon(
                    frameIndex: DiamondSkinFrameIndex,
                    itemType: Type,                    // ModItem 的 Type
                    cooldownTicks: SpecialCooldownTicks,
                    scale: 1.1f,
                    offset: new Vector2(0, -20)
                );
            }
            return true;
        }
    }
}
