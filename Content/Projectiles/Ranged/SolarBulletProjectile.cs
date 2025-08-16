using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Projectiles.Ranged
{
    // 日曜弹 射弹
    public class SolarBulletProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.light = 0.5f;
            Projectile.alpha = 255;
            Projectile.MaxUpdates = 3;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.ignoreWater = true;
            // ignore gravity
            AIType = ProjectileID.Bullet;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            var owner = Main.player[Projectile.owner];
            Projectile.position.Y -= 10f * owner.gravDir;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14.WithVolumeScale(0.5f).WithPitchOffset(0.8f), Projectile.position);
            // for (var i = 0; i < 2; i++)
            // {
            //     Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SolarFlare, 0.5f, 0.5f, 0, default, 1f);
            // }
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile p2 = Main.projectile[Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ProjectileID.DaybreakExplosion,
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner,
                    0,
                    0.85f + Main.rand.NextFloat() * 1.15f
                )];
                p2.CritChance = 0;
            }
        }

        // public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        // {
        //     if (Main.rand.NextBool(3)) // 33% chance to inflict On Fire!
        //     {
        //         target.AddBuff(BuffID.Daybreak, 180); // Inflict On Fire! for 3 seconds
        //     }
        // }
    }
}