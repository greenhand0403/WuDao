using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Systems;

namespace WuDao.Content.Global.Projectiles
{
    public class InvisibleEnemiesGlobalProjectile : GlobalProjectile
    {
        internal static bool[] HiddenHostileProjectilesThisFrame;
        internal static int[] HiddenHostileProjectileIndices;
        internal static int HiddenHostileProjectileCount;

        public override void Load()
        {
            HiddenHostileProjectilesThisFrame = new bool[Main.maxProjectiles];
            HiddenHostileProjectileIndices = new int[Main.maxProjectiles];
            HiddenHostileProjectileCount = 0;
        }

        public override void Unload()
        {
            HiddenHostileProjectilesThisFrame = null;
            HiddenHostileProjectileIndices = null;
            HiddenHostileProjectileCount = 0;
        }

        internal static bool ShouldHide(Projectile projectile)
        {
            if (!projectile.active || !projectile.hostile || projectile.friendly)
                return false;

            Player viewer = Main.LocalPlayer;
            return !InvisibleEnemies.CanSeeEcho(viewer);
        }

        public override void PostAI(Projectile projectile)
        {
            if (!ShouldHide(projectile))
                return;

            int whoAmI = projectile.whoAmI;

            if (!HiddenHostileProjectilesThisFrame[whoAmI])
            {
                HiddenHostileProjectilesThisFrame[whoAmI] = true;
                HiddenHostileProjectileIndices[HiddenHostileProjectileCount++] = whoAmI;
            }

            Rectangle oldBox = new Rectangle((int)projectile.oldPosition.X, (int)projectile.oldPosition.Y, projectile.width, projectile.height);
            Rectangle newBox = projectile.Hitbox;
            Rectangle swept = Rectangle.Union(oldBox, newBox);

            int extra = 12 + (int)projectile.velocity.Length();
            swept.Inflate(extra, extra);

            InvisibleEnemiesDustSystem.AddHiddenProjectileZone(swept);
        }
        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            if (ShouldHide(projectile))
            {
                Rectangle area = projectile.Hitbox;
                area.Inflate(24, 24);
                InvisibleEnemiesDustSystem.AddHiddenProjectileImpactZone(area);
            }

            return true;
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (projectile.hostile && !projectile.friendly)
            {
                Player viewer = Main.LocalPlayer;
                if (!InvisibleEnemies.CanSeeEcho(viewer))
                {
                    Rectangle area = projectile.Hitbox;
                    area.Inflate(28, 28);
                    InvisibleEnemiesDustSystem.AddHiddenProjectileImpactZone(area);
                }
            }
        }
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (ShouldHide(projectile))
                return false;

            return true;
        }
    }
}