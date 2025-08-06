using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;

namespace WuDao.Content.Projectiles.Ranged
{
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
            Projectile.MaxUpdates = 5;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Ranged;
            
            Projectile.ignoreWater = true;
            // ignore gravity
            AIType = ProjectileID.Bullet;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
            }
            // else
            // {
            //     SoundEngine.PlaySound(SoundID.Item10.WithVolumeScale(0.5f).WithPitchOffset(0.8f), Projectile.position);
            //     for (var i = 0; i < 4; i++)
            //     {
            //         Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SolarFlare, 0.5f, 0.5f, 0, default, 1f);
            //     }

            //     Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

            //     if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            //     {
            //         Projectile.velocity.X = -oldVelocity.X;
            //     }
            //     if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            //     {
            //         Projectile.velocity.Y = -oldVelocity.Y;
            //     }
            // }

            // 获取反射方向
            // Vector2 normal = Vector2.Zero;

            // if (Math.Abs(Projectile.velocity.X) > Math.Abs(Projectile.velocity.Y))
            // {
            //     // 横向碰撞（左右墙）
            //     normal = new Vector2(Math.Sign(Projectile.velocity.X), 0f);
            // }
            // else
            // {
            //     // 纵向碰撞（上下地板或天花板）
            //     normal = new Vector2(0f, Math.Sign(Projectile.velocity.Y));
            // }

            // // 反方向（爆炸应该往外喷）
            // Vector2 baseDir = -normal;

            // for (int i = 0; i < 5; i++)
            // {
            //     Vector2 randomOffset = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(-0.6f, 0.6f));
            //     Vector2 dustVelocity = (baseDir + randomOffset).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(10f, 20f);

            //     Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, dustVelocity, 150, default, 1.8f).noGravity = true;
            // }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10.WithVolumeScale(0.5f).WithPitchOffset(0.8f), Projectile.position);
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3)) // 33% chance to inflict On Fire!
            {
                target.AddBuff(BuffID.Daybreak, 180); // Inflict On Fire! for 3 seconds
            }
        }
    }
}