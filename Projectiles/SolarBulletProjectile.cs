using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;

namespace WuDao.Projectiles
{
    public class SolarBulletProjectile: ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            // ignore gravity
            AIType = ProjectileID.Bullet;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0) { 
                Projectile.Kill();
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item10.WithVolumeScale(0.5f).WithPitchOffset(0.8f), Projectile.position);
                for (var i = 0; i < 4; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SolarFlare, 0.5f, 0.5f, 0, default, 1f);
                }

                // Collision.HitTiles(Projectile.position,Projectile.velocity,Projectile.width,Projectile.height);
                
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                {
                    Projectile.velocity.X = -oldVelocity.X;
                }
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                }
            }
            
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10.WithVolumeScale(0.5f).WithPitchOffset(0.8f), Projectile.position);
            for (var i = 0; i < 2; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SolarFlare, 0.5f, 0.5f, 0, default, 1f);
            }
            // Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity,Projectile.width, Projectile.height);
        }
    }
}