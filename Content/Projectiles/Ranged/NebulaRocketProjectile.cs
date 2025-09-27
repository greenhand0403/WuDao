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
            ProjectileID.Sets.IsARocketThatDealsDoubleDamageToPrimaryEnemy[Type] = true;
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
        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            // 提前移动一点，对齐枪口位置
            Projectile.position += Projectile.velocity.SafeNormalize(Vector2.UnitX) * 20f;
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

                        Dust fireDust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 6f + posOffsetX, Projectile.position.Y + 6f + posOffsetY) - Projectile.velocity * 0.5f,
                            Projectile.width - 8, Projectile.height - 8, DustID.CrystalPulse2, 0f, 0f, 100);
                        fireDust.scale *= 1.2f + Main.rand.Next(10) * 0.1f;
                        fireDust.velocity *= 0.4f;
                        fireDust.noGravity = true;

                        Dust smokeDust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 3f + posOffsetX, Projectile.position.Y + 3f + posOffsetY) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Smoke, 0f, 0f, 100, default, 0.5f);
                        smokeDust.fadeIn = 1f + Main.rand.Next(5) * 0.1f;
                        smokeDust.velocity *= 0.08f;
                    }
                }

                if (Math.Abs(Projectile.velocity.X) < 15f && Math.Abs(Projectile.velocity.Y) < 15f)
                {
                    Projectile.velocity *= 1.1f;
                }
            }

            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity *= 0f;
            Projectile.alpha = 255;
            Projectile.timeLeft = 3;
            return false; // Returning false is important here. Otherwise the projectile will die without being resized (no blast radius).
        }
        public override void PrepareBombToBlow()
        {
            Projectile.tileCollide = false;
            Projectile.alpha = 255;

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
                Dust smokeDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.2f);
                smokeDust.velocity *= 2.2f;
                if (Main.rand.NextBool(2))
                {
                    smokeDust.scale = 0.5f;
                    smokeDust.fadeIn = 1f + Main.rand.NextFloat(1.0f);
                }
            }
            for (int j = 0; j < 70; j++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.UndergroundHallowedEnemies, 0f, 0f, 100, default, 2.5f);
                dust.noGravity = true;
                dust.velocity *= 3.2f;
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CrystalPulse2, 0f, 0f, 100, default(Color), 1.5f);
                dust.velocity *= 1.6f;
                dust.noGravity = true;
            }

            for (int k = 0; k < 3; k++)
            {
                float speedMulti = 0.3f;
                if (k == 1)
                {
                    speedMulti = 0.6f;
                }
                else if (k == 2)
                {
                    speedMulti = 0.9f;
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