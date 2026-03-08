using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Enemy
{
    public static class BloodstainedIntestineWormAI
    {
        /// <summary>
        /// 普通移动速度
        /// </summary>
        private const float WormSpeed = 12f;
        /// <summary>
        /// 冲刺加速度
        /// </summary>
        private const float WormAcceleration = 0.15f;
        /// <summary>
        /// 体节间距
        /// </summary>
        private const float SegmentSpacing = 18f;

        public static void HeadAI(NPC npc)
        {
            if (!npc.HasValidTarget || Main.player[npc.target].dead)
            {
                npc.TargetClosest();
            }

            Player target = Main.player[npc.target];
            if (!target.active || target.dead)
            {
                npc.velocity.Y += 0.2f;
                if (npc.timeLeft > 60)
                    npc.timeLeft = 60;

                if (npc.velocity.LengthSquared() > 0.04f)
                    npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;

                return;
            }

            bool insideSolid = IsInsideSolidOrLiquid(npc);

            float distanceToTarget = Vector2.Distance(npc.Center, target.Center);

            // 靠近玩家后不要继续长时间潜地，直接切到出土地表的运动模式
            if (distanceToTarget < 200f)
            {
                insideSolid = false;
            }

            Vector2 npcCenter = npc.Center;
            Vector2 targetCenter = target.Center;

            // 像原版吞世怪一样，把目标点和自身点吸附到 16x16 网格
            npcCenter.X = (int)(npcCenter.X / 16f) * 16f;
            npcCenter.Y = (int)(npcCenter.Y / 16f) * 16f;
            targetCenter.X = (int)(targetCenter.X / 16f) * 16f;
            targetCenter.Y = (int)(targetCenter.Y / 16f) * 16f;

            Vector2 toTarget = targetCenter - npcCenter;

            if (insideSolid)
            {
                MoveLikeDiggingWorm(npc, toTarget);
            }
            else
            {
                MoveLikeFallingWorm(npc, toTarget);
            }

            if (npc.velocity.LengthSquared() > 0.04f)
            {
                npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
            }

            npc.spriteDirection = npc.direction = npc.velocity.X > 0f ? 1 : -1;
        }

        public static void BodyOrTailAI(NPC npc)
        {
            int prevIndex = (int)npc.ai[1];
            int headIndex = (int)npc.ai[2];

            if (!IsValidSegment(prevIndex) || !IsValidSegment(headIndex))
            {
                npc.active = false;
                return;
            }

            NPC prev = Main.npc[prevIndex];
            NPC head = Main.npc[headIndex];

            if (!head.active)
            {
                npc.active = false;
                return;
            }

            npc.realLife = head.whoAmI;

            Vector2 center = npc.Center;
            Vector2 prevCenter = prev.Center;
            Vector2 diff = prevCenter - center;

            if (diff == Vector2.Zero)
                diff = Vector2.UnitY;

            float distance = diff.Length();
            float desiredDistance = (prev.width + npc.width) * 0.5f;
            if (desiredDistance < SegmentSpacing)
                desiredDistance = SegmentSpacing;

            float moveFactor = (distance - desiredDistance) / distance;
            Vector2 move = diff * moveFactor;

            npc.velocity = Vector2.Zero;
            npc.position += move;

            diff = prev.Center - npc.Center;
            if (diff != Vector2.Zero)
                npc.rotation = diff.ToRotation() + MathHelper.PiOver2;

            npc.spriteDirection = prev.spriteDirection;
            npc.direction = prev.direction;
        }

        public static bool IsInsideSolidOrLiquid(NPC npc)
        {
            // 用更小的探测框，避免头已经快出土了，
            // 但因为大碰撞箱边缘蹭到方块，仍被判定为“在地下”
            Rectangle probeBox = new Rectangle(
                (int)npc.Center.X - 10,
                (int)npc.Center.Y - 10,
                20,
                20
            );

            int left = probeBox.Left / 16 - 1;
            int right = probeBox.Right / 16 + 2;
            int top = probeBox.Top / 16 - 1;
            int bottom = probeBox.Bottom / 16 + 2;

            if (left < 0) left = 0;
            if (right > Main.maxTilesX) right = Main.maxTilesX;
            if (top < 0) top = 0;
            if (bottom > Main.maxTilesY) bottom = Main.maxTilesY;

            for (int x = left; x < right; x++)
            {
                for (int y = top; y < bottom; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile == null)
                        continue;

                    bool solidTile =
                        tile.HasTile &&
                        Main.tileSolid[tile.TileType] &&
                        (!Main.tileSolidTop[tile.TileType] || tile.TileFrameY != 0);

                    // 先别把液体也算进去，不然更容易一直判定在地下
                    if (!solidTile)
                        continue;

                    Rectangle tileRect = new Rectangle(x * 16, y * 16, 16, 16);
                    if (probeBox.Intersects(tileRect))
                        return true;
                }
            }

            return false;
        }

        private static void MoveLikeDiggingWorm(NPC npc, Vector2 toTarget)
        {
            float distance = toTarget.Length();
            if (distance < 0.001f)
                distance = 0.001f;

            Vector2 desiredVelocity = toTarget / distance * WormSpeed;

            float absX = Math.Abs(desiredVelocity.X);
            float absY = Math.Abs(desiredVelocity.Y);

            if ((npc.velocity.X > 0f && desiredVelocity.X > 0f) ||
                (npc.velocity.X < 0f && desiredVelocity.X < 0f) ||
                (npc.velocity.Y > 0f && desiredVelocity.Y > 0f) ||
                (npc.velocity.Y < 0f && desiredVelocity.Y < 0f))
            {
                if (npc.velocity.X < desiredVelocity.X)
                    npc.velocity.X += WormAcceleration;
                else if (npc.velocity.X > desiredVelocity.X)
                    npc.velocity.X -= WormAcceleration;

                if (npc.velocity.Y < desiredVelocity.Y)
                    npc.velocity.Y += WormAcceleration;
                else if (npc.velocity.Y > desiredVelocity.Y)
                    npc.velocity.Y -= WormAcceleration;

                if (Math.Abs(desiredVelocity.Y) < WormSpeed * 0.2f &&
                    ((npc.velocity.X > 0f && desiredVelocity.X < 0f) || (npc.velocity.X < 0f && desiredVelocity.X > 0f)))
                {
                    npc.velocity.Y += npc.velocity.Y > 0f ? WormAcceleration * 2f : -WormAcceleration * 2f;
                }

                if (Math.Abs(desiredVelocity.X) < WormSpeed * 0.2f &&
                    ((npc.velocity.Y > 0f && desiredVelocity.Y < 0f) || (npc.velocity.Y < 0f && desiredVelocity.Y > 0f)))
                {
                    npc.velocity.X += npc.velocity.X > 0f ? WormAcceleration * 2f : -WormAcceleration * 2f;
                }
            }
            else if (absX > absY)
            {
                if (npc.velocity.X < desiredVelocity.X)
                    npc.velocity.X += WormAcceleration * 1.1f;
                else if (npc.velocity.X > desiredVelocity.X)
                    npc.velocity.X -= WormAcceleration * 1.1f;

                if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < WormSpeed * 0.5f)
                    npc.velocity.Y += npc.velocity.Y > 0f ? WormAcceleration : -WormAcceleration;
            }
            else
            {
                if (npc.velocity.Y < desiredVelocity.Y)
                    npc.velocity.Y += WormAcceleration * 1.1f;
                else if (npc.velocity.Y > desiredVelocity.Y)
                    npc.velocity.Y -= WormAcceleration * 1.1f;

                if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < WormSpeed * 0.5f)
                    npc.velocity.X += npc.velocity.X > 0f ? WormAcceleration : -WormAcceleration;
            }

            float speed = npc.velocity.Length();
            if (speed > WormSpeed)
                npc.velocity *= WormSpeed / speed;
        }

        private static void MoveLikeFallingWorm(NPC npc, Vector2 toTarget)
        {
            npc.velocity.Y += 0.11f;
            if (npc.velocity.Y > WormSpeed)
                npc.velocity.Y = WormSpeed;

            if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < WormSpeed * 0.4f)
            {
                npc.velocity.X += npc.velocity.X < 0f
                    ? -WormAcceleration * 1.1f
                    : WormAcceleration * 1.1f;
            }
            else if (npc.velocity.Y >= WormSpeed)
            {
                if (npc.velocity.X < toTarget.X)
                    npc.velocity.X += WormAcceleration;
                else if (npc.velocity.X > toTarget.X)
                    npc.velocity.X -= WormAcceleration;
            }
            else if (npc.velocity.Y > 4f)
            {
                npc.velocity.X += npc.velocity.X < 0f
                    ? WormAcceleration * 0.9f
                    : -WormAcceleration * 0.9f;
            }
        }

        public static bool IsValidSegment(int index)
        {
            return index >= 0 && index < Main.maxNPCs && Main.npc[index].active;
        }

        public static void SpawnSegments(NPC head, int bodyType, int tailType, int bodyCount)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            head.realLife = head.whoAmI;

            int latest = head.whoAmI;

            for (int i = 0; i < bodyCount; i++)
            {
                int bodyIndex = NPC.NewNPC(
                    head.GetSource_FromAI(),
                    (int)head.Center.X,
                    (int)head.Center.Y,
                    bodyType,
                    Target: head.target
                );

                NPC body = Main.npc[bodyIndex];
                body.realLife = head.whoAmI;
                body.ai[1] = latest;        // 前一节
                body.ai[2] = head.whoAmI;   // 头部
                Main.npc[latest].ai[0] = bodyIndex; // 后一节
                body.netUpdate = true;

                latest = bodyIndex;
            }

            int tailIndex = NPC.NewNPC(
                head.GetSource_FromAI(),
                (int)head.Center.X,
                (int)head.Center.Y,
                tailType,
                Target: head.target
            );

            NPC tail = Main.npc[tailIndex];
            tail.realLife = head.whoAmI;
            tail.ai[1] = latest;
            tail.ai[2] = head.whoAmI;
            Main.npc[latest].ai[0] = tailIndex;
            tail.netUpdate = true;

            head.netUpdate = true;
        }
    }

    public abstract class BloodstainedIntestineSegment : ModNPC
    {
        protected virtual bool IsHead => false;

        public override void SetDefaults()
        {
            NPC.width = 44;
            NPC.height = 44;
            NPC.damage = 22;
            NPC.defense = 8;
            NPC.lifeMax = 12000;

            NPC.knockBackResist = 0f;
            NPC.value = 150f;

            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.behindTiles = true;
            NPC.netAlways = true;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }

        public override void AI()
        {
            if (IsHead)
                BloodstainedIntestineWormAI.HeadAI(NPC);
            else
                BloodstainedIntestineWormAI.BodyOrTailAI(NPC);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Bleeding, 180);
            target.AddBuff(BuffID.Weak, 180);
        }

        public override bool CheckActive() => false;
    }

    public class BloodstainedIntestineHead : BloodstainedIntestineSegment
    {
        protected override bool IsHead => true;

        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.width = 44;
            NPC.height = 44;
            NPC.damage = 28;
            NPC.defense = 10;
            NPC.lifeMax = Main.hardMode ? 26000 : 18000;

            NPC.aiStyle = -1;

            // 视觉上等价于“碰撞箱向上对齐一点”
            DrawOffsetY = 2;
        }
        // 仅在猩红地下区域生成
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneCrimson && spawnInfo.Player.ZoneRockLayerHeight)
                return Main.hardMode ? 0.12f : 0.06f;

            return 0f;
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0f)
            {
                NPC.localAI[0] = 1f;

                // 总段数（含头尾）
                int totalSegments = Main.hardMode
                    ? Main.rand.Next(8, 11)  // 8~10
                    : Main.rand.Next(6, 9);  // 6~8

                int bodyCount = totalSegments - 2;
                BloodstainedIntestineWormAI.SpawnSegments(
                    NPC,
                    ModContent.NPCType<BloodstainedIntestineBody>(),
                    ModContent.NPCType<BloodstainedIntestineTail>(),
                    bodyCount
                );
            }

            base.AI();
        }
    }

    public class BloodstainedIntestineBody : BloodstainedIntestineSegment
    {
        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.width = 44;
            NPC.height = 44;
            NPC.damage = 20;
            NPC.defense = 8;
            NPC.lifeMax = Main.hardMode ? 18000 : 11000;

            NPC.aiStyle = -1;
        }
    }

    public class BloodstainedIntestineTail : BloodstainedIntestineSegment
    {
        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.width = 44;
            NPC.height = 44;
            NPC.damage = 16;
            NPC.defense = 6;
            NPC.lifeMax = Main.hardMode ? 15000 : 9000;

            NPC.aiStyle = -1;
        }
    }
}