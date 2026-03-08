using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles.Summon
{
    public class DandelionSentry : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.Dandelion;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = Main.npcFrameCount[NPCID.Dandelion];
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;

            Projectile.friendly = true;

            Projectile.sentry = true;

            Projectile.DamageType = DamageClass.Summon;

            Projectile.timeLeft = Projectile.SentryLifeTime;

            Projectile.penetrate = -1;

            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            Projectile.ai[0]++;

            if (Projectile.ai[0] > 60)
            {
                Projectile.ai[0] = 0;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-2, 2), -5);

                    int dandelionSeed = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        velocity,
                        ProjectileID.DandelionSeed,
                        Projectile.damage,
                        0,
                        Projectile.owner
                    );
                    Main.projectile[dandelionSeed].hostile = false;
                    Main.projectile[dandelionSeed].friendly = true;
                }
            }
        }
    }
}