using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Projectiles.Summon
{
    public class ZombieMinion : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.Zombie;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            Main.projPet[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 48;

            Projectile.friendly = true;

            Projectile.minion = true;

            Projectile.minionSlots = 1f;

            Projectile.penetrate = -1;

            Projectile.timeLeft = 18000;

            Projectile.DamageType = DamageClass.Summon;

            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            float distance = 700f;

            NPC target = null;

            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC npc = Main.npc[k];

                if (npc.CanBeChasedBy(this))
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);

                    if (dist < distance)
                    {
                        distance = dist;
                        target = npc;
                    }
                }
            }

            if (target != null)
            {
                float speed = 4f;

                if (target.Center.X > Projectile.Center.X)
                    Projectile.velocity.X = speed;
                else
                    Projectile.velocity.X = -speed;
            }
            else
            {
                Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center + new Vector2(-40, 0), 0.05f);
            }
        }
    }
}