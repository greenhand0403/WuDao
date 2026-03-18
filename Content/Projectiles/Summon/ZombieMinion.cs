using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles.Summon
{
    public class ZombieMinion : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.TheGroom;
        private const float Gravity = 0.4f;
        private const float MaxFallSpeed = 10f;
        private const float IdleSpeed = 1.8f;
        private const float ChaseSpeed = 2.4f;
        private const float TeleportDistance = 1200f;
        private const float SearchDistance = 700f;
        private const int StuckJumpTime = 20; // 卡住约 1/3 秒后尝试强制跳
        private const float StuckMoveThreshold = 0.6f;
        // 是不是僵尸新娘
        private bool IsBride => false;
        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
        }

        public override void SetDefaults()
        {
            // 默认僵尸新郎
            Projectile.width = 18;
            Projectile.height = 40;

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;

            Projectile.DamageType = DamageClass.Summon;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.netImportant = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.X != Projectile.velocity.X)
                Projectile.velocity.X = 0f;

            if (oldVelocity.Y > 0f && Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = 0f;

            return false;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || !player.HasBuff(ModContent.BuffType<ZombieMinionBuff>()))
            {
                player.ClearBuff(ModContent.BuffType<ZombieMinionBuff>());
                Projectile.Kill();
                return;
            }

            // 维持buff（经典写法）
            if (player.HasBuff(ModContent.BuffType<ZombieMinionBuff>()))
                Projectile.timeLeft = 2;

            if (Vector2.Distance(Projectile.Center, player.Center) > TeleportDistance)
            {
                Projectile.Center = player.Bottom + new Vector2(-player.direction * 20f, -Projectile.height * 0.5f);
                Projectile.velocity = Vector2.Zero;
                Projectile.localAI[0] = 0f;
                Projectile.netUpdate = true;
            }

            NPC target = FindTarget(player);

            ApplyGravity();

            Vector2 moveTarget;
            float moveSpeed;
            bool shouldForceMove;

            if (target != null)
            {
                moveTarget = target.Center;
                moveSpeed = ChaseSpeed;
                shouldForceMove = true;
            }
            else
            {
                moveTarget = player.Bottom + new Vector2(-player.direction * 48f, -Projectile.height * 0.5f);
                moveSpeed = IdleSpeed;
                shouldForceMove = Vector2.Distance(Projectile.Center, moveTarget) > 64f;
            }

            GroundMove(moveTarget, moveSpeed, shouldForceMove);

            UpdateDirection();
            FindFrameLikeZombie();

            // 记录上一帧位置给卡住检测用
            Projectile.localAI[1] = Projectile.position.X;
        }

        private NPC FindTarget(Player player)
        {
            NPC target = null;
            float bestDist = SearchDistance;

            if (player.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy(this))
                {
                    float d = Vector2.Distance(Projectile.Center, npc.Center);
                    if (d <= bestDist + 250f)
                        return npc;
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                    continue;

                float d = Vector2.Distance(Projectile.Center, npc.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    target = npc;
                }
            }

            return target;
        }

        private void ApplyGravity()
        {
            Projectile.velocity.Y += Gravity;
            if (Projectile.velocity.Y > MaxFallSpeed)
                Projectile.velocity.Y = MaxFallSpeed;
        }

        private void GroundMove(Vector2 destination, float moveSpeed, bool shouldForceMove)
        {
            float dx = destination.X - Projectile.Center.X;
            int dir = dx > 8f ? 1 : (dx < -8f ? -1 : 0);

            if (dir == 0)
            {
                Projectile.velocity.X *= 0.8f;
                Projectile.localAI[0] = 0f;
                return;
            }

            Projectile.velocity.X = dir * moveSpeed;

            if (!IsOnGround())
            {
                Projectile.localAI[0] = 0f;
                return;
            }

            if (ShouldJumpOneTile(dir))
            {
                Projectile.velocity.Y = -6.2f;
                Projectile.netUpdate = true;
                Projectile.localAI[0] = 0f;
                return;
            }

            if (ShouldJumpTwoTiles(dir))
            {
                Projectile.velocity.Y = -7.2f;
                Projectile.netUpdate = true;
                Projectile.localAI[0] = 0f;
                return;
            }

            if (destination.Y + 10f < Projectile.Center.Y && Math.Abs(dx) < 120f)
            {
                Projectile.velocity.Y = -6.8f;
                Projectile.netUpdate = true;
                Projectile.localAI[0] = 0f;
                return;
            }

            HandleStuckJump(dir, moveSpeed, shouldForceMove);
        }

        private void HandleStuckJump(int dir, float moveSpeed, bool shouldForceMove)
        {
            if (!shouldForceMove || dir == 0)
            {
                Projectile.localAI[0] = 0f;
                return;
            }

            float movedX = Math.Abs(Projectile.position.X - Projectile.localAI[1]);
            bool barelyMoved = movedX < StuckMoveThreshold;
            bool tryingToMove = Math.Abs(Projectile.velocity.X) > 1f;

            if (tryingToMove && barelyMoved)
                Projectile.localAI[0]++;
            else
                Projectile.localAI[0] = 0f;

            if (Projectile.localAI[0] >= StuckJumpTime)
            {
                Projectile.velocity.X = dir * (moveSpeed + 1.2f);
                Projectile.velocity.Y = -7.4f;
                Projectile.localAI[0] = 0f;
                Projectile.netUpdate = true;
            }
        }

        private bool IsOnGround()
        {
            int x = (int)(Projectile.Center.X / 16f);
            int y = (int)((Projectile.Bottom.Y + 2f) / 16f);

            return HasSolidTile(x, y) || HasSolidTile(x - 1, y) || HasSolidTile(x + 1, y);
        }

        private bool ShouldJumpOneTile(int dir)
        {
            Point foot = Projectile.Bottom.ToTileCoordinates();

            int x = foot.X + dir;
            int y0 = foot.Y - 1;
            int y1 = foot.Y - 2;
            int y2 = foot.Y - 3;

            if (!WorldGen.InWorld(x, y0, 10))
                return false;

            bool blockFeet = HasSolidTile(x, y0);
            bool blockBody = HasSolidTile(x, y1);
            bool openHead = !HasSolidTile(x, y2);

            return blockFeet && !blockBody && openHead;
        }

        private bool ShouldJumpTwoTiles(int dir)
        {
            Point foot = Projectile.Bottom.ToTileCoordinates();

            int x1 = foot.X + dir;
            int x2 = foot.X + dir * 2;

            int y0 = foot.Y - 1;
            int y1 = foot.Y - 2;
            int y2 = foot.Y - 3;
            int groundY = foot.Y;

            if (!WorldGen.InWorld(x1, y0, 10) || !WorldGen.InWorld(x2, y0, 10))
                return false;

            bool wall1Low = HasSolidTile(x1, y0);
            bool wall1Mid = HasSolidTile(x1, y1);
            bool open1Top = !HasSolidTile(x1, y2);

            bool ground2 = HasSolidTile(x2, groundY);
            bool open2Mid = !HasSolidTile(x2, y1);

            return (wall1Low && wall1Mid && open1Top) || (wall1Low && ground2 && open2Mid);
        }

        private bool HasSolidTile(int x, int y)
        {
            if (!WorldGen.InWorld(x, y, 10))
                return false;

            Tile tile = Main.tile[x, y];
            if (tile == null || !tile.HasTile)
                return false;

            return Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
        }

        private void UpdateDirection()
        {
            if (Projectile.velocity.X > 0.05f)
                Projectile.spriteDirection = -1;
            else if (Projectile.velocity.X < -0.05f)
                Projectile.spriteDirection = 1;
        }

        private void FindFrameLikeZombie()
        {
            if (!IsOnGround())
            {
                Projectile.frame = 1;
                Projectile.frameCounter = 0;
                return;
            }

            if (Math.Abs(Projectile.velocity.X) < 0.2f)
            {
                Projectile.frame = 0;
                Projectile.frameCounter = 0;
                return;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 8)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 3)
                    Projectile.frame = 0;
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] == 1f)
            {
                Texture2D tex = ModContent.Request<Texture2D>("Terraria/Images/NPC_" + (int)NPCID.TheBride).Value;
                // 手动绘制僵尸新娘 34x156 3帧
                int frame = (int)((Main.GameUpdateCount / 4) % 3);
                // 如果速度太小就使用第1帧
                if (Math.Abs(Projectile.velocity.X) < 0.2f)
                    frame = 0;
                Rectangle src = new Rectangle(0, frame * 52, 34, 52);
                Vector2 origin = src.Size() * 0.5f;
                Main.EntitySpriteDraw(
                    tex,
                    Projectile.Center - Main.screenPosition,
                    src,
                    lightColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
                );
            }
            else
            {
                Texture2D tex = ModContent.Request<Texture2D>("Terraria/Images/NPC_" + (int)NPCID.TheGroom).Value;
                // 手动绘制僵尸新郎 34x164 3帧
                int frame = (int)((Main.GameUpdateCount / 4) % 3);
                // 如果速度太小就使用第1帧
                if (Math.Abs(Projectile.velocity.X) < 0.2f)
                    frame = 0;
                Rectangle src = new Rectangle(0, frame * 54, 34, 54);
                Vector2 origin = src.Size() * 0.5f;
                Main.EntitySpriteDraw(
                    tex,
                    Projectile.Center - Main.screenPosition,
                    src,
                    lightColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
                );
            }
            
            return false;
        }
    }
}