using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using WuDao.Content.Players;

namespace WuDao.Content.Global.Projectiles
{
    // 压制力场：削弱敌方射弹
    public class DevolutionGlobalProj : GlobalProjectile
    {
        public override void ModifyHitPlayer(Projectile proj, Player target, ref Player.HurtModifiers modifiers)
        {
            if (!proj.hostile) return; // 只影响敌方弹幕
                                       // 敌方弹幕以“离最近的持有者”的判断来近似屏幕范围
                                       // 屏幕半径（近似）：用对角线一半，或你自定常量例如 900f
            float radius = (float)Math.Sqrt(Main.screenWidth * Main.screenWidth + Main.screenHeight * Main.screenHeight) * 0.5f;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player plr = Main.player[i];
                if (plr == null || !plr.active) continue;

                if (plr.GetModPlayer<DevolutionPlayer>().HasDevolutionAura)
                {
                    if (Vector2.Distance(plr.Center, proj.Center) <= radius)
                    {
                        modifiers.SourceDamage *= 0.8f;
                        break;
                    }
                }
            }
        }
    }

}