using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WuDao.Common;

namespace WuDao.Content.Global.Projectiles
{
    public class InvisibleEnemiesGlobalProjectile : GlobalProjectile
    {
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (projectile.active && projectile.hostile && !projectile.friendly)
            {
                var viewer = Main.LocalPlayer;
                if (!InvisibleEnemies.CanSeeEcho(viewer))
                {
                    return false;
                }
            }
            return true;
        }
    }
}