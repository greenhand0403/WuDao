using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Projectiles.Ranged
{
    public class NebulaRocketProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsARocketThatDealsDoubleDamageToPrimaryEnemy[Type] = true; // Deals double damage on direct hits.
            //ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true; // Damage dealt to players does not scale with difficulty in vanilla.
            ProjectileID.Sets.Explosive[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
            {
                Projectile.PrepareBombToBlow();
            }
            else
            {
                if (Math.Abs(Projectile.velocity.X) >= 8f || Math.Abs(Projectile.velocity.Y) >= 8f)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        float posOffsetX = 0f;
                        float posOffsetY = 0f;
                        if (i == 1)
                        {
                            posOffsetX = Projectile.velocity.X * 0.5f;
                            posOffsetY = Projectile.velocity.Y * 0.5f;
                        }

                        // Spawn fire dusts at the back of the rocket.
                        Dust fireDust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 6f + posOffsetX, Projectile.position.Y + 6f + posOffsetY) - Projectile.velocity * 0.5f,
                            Projectile.width - 8, Projectile.height - 8, DustID.CrystalPulse2, 0f, 0f, 100);
                        fireDust.scale *= 2f + Main.rand.Next(10) * 0.1f;
                        fireDust.velocity *= 0.2f;
                        fireDust.noGravity = true;

                        // Spawn smoke dusts at the back of the rocket.
                        Dust smokeDust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 3f + posOffsetX, Projectile.position.Y + 3f + posOffsetY) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Smoke, 0f, 0f, 100, default, 0.5f);
                        smokeDust.fadeIn = 1f + Main.rand.Next(5) * 0.1f;
                        smokeDust.velocity *= 0.05f;
                    }
                }

                if (Math.Abs(Projectile.velocity.X) < 15f && Math.Abs(Projectile.velocity.Y) < 15f)
                {
                    Projectile.velocity *= 1.1f;
                }
            }
            // Rotate the rocket in the direction that it is moving.
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity *= 0f; // Stop moving so the explosion is where the rocket was.
            Projectile.alpha = 255;
            Projectile.timeLeft = 3; // Set the timeLeft to 3 so it can get ready to explode.
            return false; // Returning false is important here. Otherwise the projectile will die without being resized (no blast radius).
        }
        public override void PrepareBombToBlow()
        {
            Projectile.tileCollide = false; // This is important or the explosion will be in the wrong place if the rocket explodes on slopes.
            Projectile.alpha = 255; // Make the rocket invisible.

            // Resize the hitbox of the projectile for the blast "radius".
            // Rocket I: 128, Rocket III: 200, Mini Nuke Rocket: 250
            // Measurements are in pixels, so 200 / 16 = 12.5 tiles.
            Projectile.Resize(200, 200);
            // Set the knockback of the blast.
            // Rocket I: 8f, Rocket III: 10f, Mini Nuke Rocket: 12f
            Projectile.knockBack = 10f;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14.WithVolumeScale(0.5f).WithPitchOffset(0.8f), Projectile.position);

            Projectile.Resize(80, 80);

            for (var i = 0; i < 40; i++)
            {
                Dust smokeDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.4f);
                smokeDust.velocity *= 3f;
                if (Main.rand.NextBool(2))
                {
                    smokeDust.scale = 0.5f;
                    smokeDust.fadeIn = 1f + Main.rand.NextFloat(1.0f);
                }
            }
            for (int j = 0; j < 70; j++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.UndergroundHallowedEnemies, 0f, 0f, 100, default, 3f);
                if (Main.rand.NextBool(3))
                {
                    dust.noGravity = true;
                }
                dust.velocity *= 5f;
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Gastropod, 0f, 0f, 100, default(Color), 2f);
                dust.velocity *= 2f;
            }

            // Spawn a bunch of smoke gores.
            for (int k = 0; k < 3; k++)
            {
                float speedMulti = 0.33f;
                if (k == 1)
                {
                    speedMulti = 0.66f;
                }else if (k == 2)
                {
                    speedMulti = 1f;
                }

                Gore smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
                smokeGore.velocity *= speedMulti;
                smokeGore.velocity += Vector2.One;
                smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
                smokeGore.velocity *= speedMulti;
                smokeGore.velocity.X -= 1f;
                smokeGore.velocity.Y += 1f;
                smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
                smokeGore.velocity *= speedMulti;
                smokeGore.velocity.X += 1f;
                smokeGore.velocity.Y -= 1f;
                smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
                smokeGore.velocity *= speedMulti;
                smokeGore.velocity -= Vector2.One;
            }

            Projectile.Resize(16, 16);
        }
    }
}