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
    public class DiamondSkin : JuexueItem
    {
        public override int QiCost => 100;
        public override int SpecialCooldownTicks => 60 * 60; // 60 秒
        public const int DiamondSkinFrameIndex = 2;
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            int time = 60 * Math.Max(Helpers.BossProgressPower.GetUniqueBossCount(), 5);
            player.AddBuff(ModContent.BuffType<DiamondSkinBuff>(), time);
            SoundEngine.PlaySound(SoundID.Item29, player.Center);
            if (!Main.dedServ)
            {
                // 触发 2 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
                qi.TriggerJuexueGhost(DiamondSkinFrameIndex, durationTick: time, scale: 1.1f, offset: new Vector2(0, -16));
            }
            return true;
        }
    }
}
