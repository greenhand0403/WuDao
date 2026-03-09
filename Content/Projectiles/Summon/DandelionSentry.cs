using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Summon
{
    public class DandelionSentry : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.Dandelion;

        private const int IdleStart = 0;
        private const int IdleEnd = 6;
        private const int AttackStart = 7;
        private const int AttackEnd = 12;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = Main.npcFrameCount[NPCID.Dandelion];
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 40;
            
            Projectile.friendly = true;
            Projectile.sentry = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.penetrate = -1;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            Projectile.velocity = Vector2.Zero;

            NPC target = FindTarget(player, 560f);

            if (target != null)
            {
                Projectile.direction = target.Center.X >= Projectile.Center.X ? 1 : -1;
                Projectile.spriteDirection = Projectile.direction;

                Projectile.ai[0]++;
                if (Projectile.ai[0] >= 45f)
                {
                    Projectile.ai[0] = 0f;
                    FireSeedAt(target);
                }

                AnimateAttack();
            }
            else
            {
                Projectile.ai[0] = 0f;
                AnimateIdle();
            }
        }

        private NPC FindTarget(Player player, float maxDistance)
        {
            NPC target = null;
            float bestDistance = maxDistance;

            if (player.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy(this))
                {
                    float d = Vector2.Distance(Projectile.Center, npc.Center);
                    if (d <= maxDistance)
                        return npc;
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                    continue;

                float d = Vector2.Distance(Projectile.Center, npc.Center);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    target = npc;
                }
            }

            return target;
        }

        private void FireSeedAt(NPC target)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            Vector2 spawnPos = Projectile.Center + new Vector2(Projectile.spriteDirection * 10f, -8f);
            Vector2 velocity = target.Center - spawnPos;
            if (velocity == Vector2.Zero)
                velocity = -Vector2.UnitY;

            velocity.Normalize();
            velocity *= 6.5f;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos,
                velocity,
                ModContent.ProjectileType<DandelionSeedFriendly>(),
                Projectile.damage,
                0.5f,
                Projectile.owner
            );
        }
// 0~6 空闲帧
        private void AnimateIdle()
        {
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] >= 8f)
            {
                Projectile.localAI[0] = 0f;
                Projectile.frame++;
                if (Projectile.frame < IdleStart || Projectile.frame > IdleEnd)
                    Projectile.frame = IdleStart;
            }
        }
// 7~12 攻击帧
        private void AnimateAttack()
        {
            Projectile.localAI[0]++;
            if (Projectile.frame < AttackStart || Projectile.frame > AttackEnd)
                Projectile.frame = AttackStart;

            if (Projectile.localAI[0] >= 6f)
            {
                Projectile.localAI[0] = 0f;
                Projectile.frame++;
                if (Projectile.frame > AttackEnd)
                    Projectile.frame = AttackStart;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Type];
            Rectangle source = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                source,
                lightColor,
                0f,
                origin,
                1f,
                effects,
                0
            );

            return false;
        }
    }
}