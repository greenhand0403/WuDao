using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace WuDao.Content.Enemy
{
    public class AngryHunterHead : ModNPC
    {
        public const int TentacleCount = 3;
        public const float MaxVineLength = 55f * 16f; // 55格

        private const string TentacleTexturePath = "WuDao/Content/Enemy/AngryHunterTentacle";

        // 三个视觉触手的锚点数据
        private readonly Point[] _anchorTiles = new Point[TentacleCount];
        private readonly Vector2[] _anchorWorld = new Vector2[TentacleCount];
        private readonly bool[] _anchorValid = new bool[TentacleCount];
        private readonly int[] _anchorTimer = new int[TentacleCount];
        private readonly Vector2[] _anchorDrawWorld = new Vector2[TentacleCount];   // 触手头当前绘制位置
        private readonly Vector2[] _anchorMoveTarget = new Vector2[TentacleCount];  // 触手头要移动到的新位置
        private readonly bool[] _anchorMoving = new bool[TentacleCount];            // 是否正在移动
        private readonly float[] _anchorMoveSpeed = new float[TentacleCount];       // 每根触手移动速度
        public override void SetStaticDefaults()
        {
            // 你的头部贴图是 3 帧竖排，总高 168，单帧 100x56
            Main.npcFrameCount[NPC.type] = 3;
        }

        public override void SetDefaults()
        {
            // 命中箱建议小于贴图本体
            NPC.width = 60;
            NPC.height = 60;

            NPC.damage = 200;
            NPC.defense = 20;
            NPC.lifeMax = 900;

            NPC.knockBackResist = 0.45f;

            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.value = Item.buyPrice(0, 0, 2, 50);
            NPC.HitSound = SoundID.NPCHit8;
            NPC.DeathSound = SoundID.NPCDeath8;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneJungle)
                return 0.12f;

            return 0f;
        }

        public override bool CheckActive() => true;

        public override void AI()
        {
            NPC.TargetClosest(faceTarget: false);
            Player player = Main.player[NPC.target];

            if (!player.active || player.dead)
            {
                NPC.velocity.Y -= 0.08f;
                NPC.EncourageDespawn(10);
                return;
            }
            // 距离太远则消失
            if (Vector2.Distance(NPC.Center, player.Center) > 3000f)
            {
                NPC.EncourageDespawn(10);
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                UpdateAnchors(player);
            }

            UpdateAnchorVisuals();
            // ===== 1) 取三个锚点平均中心 =====
            Vector2 anchorCenter = Vector2.Zero;
            int validCount = 0;

            for (int i = 0; i < TentacleCount; i++)
            {
                if (_anchorValid[i])
                {
                    anchorCenter += _anchorWorld[i];
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                anchorCenter = NPC.Center;
            }
            else
            {
                anchorCenter /= validCount;
            }

            // ===== 2) 模仿世纪之花一阶段：目标点 = 锚点平均中心 + 朝玩家偏移 =====
            Vector2 offsetToPlayer = player.Center - anchorCenter;

            float maxOffsetDistance = 500f; // 类似原版 num784
            float maxSpeed = 2.5f;          // 类似原版 num779
            float accel = 0.025f;           // 类似原版 num780

            if (NPC.life < NPC.lifeMax / 2)
            {
                maxSpeed = 5f;
                accel = 0.05f;
            }

            if (NPC.life < NPC.lifeMax / 4)
            {
                maxSpeed = 7f;
            }

            // 玩家不在丛林时狂暴一点，可按需要删掉
            bool enraged = !player.ZoneJungle ||
                           player.position.Y < Main.worldSurface * 16f ||
                           player.position.Y > Main.UnderworldLayer * 16f;

            if (enraged)
            {
                maxOffsetDistance += 350f;
                maxSpeed += 8f;
                accel = 0.15f;
            }

            if (Main.expertMode)
            {
                maxOffsetDistance += 150f;
                maxSpeed += 1f;
                maxSpeed *= 1.1f;
                accel += 0.01f;
                accel *= 1.1f;
            }

            if (Main.getGoodWorld)
            {
                maxSpeed *= 1.15f;
                accel *= 1.15f;
            }

            if (offsetToPlayer.Length() > maxOffsetDistance)
            {
                offsetToPlayer = Vector2.Normalize(offsetToPlayer) * maxOffsetDistance;
            }

            Vector2 desiredPos = anchorCenter + offsetToPlayer;

            // ===== 3) 把目标点转成目标速度 =====
            Vector2 toDesired = desiredPos - NPC.Center;
            Vector2 desiredVelocity;

            float distance = toDesired.Length();
            if (distance < maxSpeed)
            {
                desiredVelocity = NPC.velocity;
            }
            else
            {
                desiredVelocity = toDesired / distance * maxSpeed;
            }

            // ===== 4) 用原版类似的“加减速逼近目标速度” =====
            if (NPC.velocity.X < desiredVelocity.X)
            {
                NPC.velocity.X += accel;
                if (NPC.velocity.X < 0f && desiredVelocity.X > 0f)
                    NPC.velocity.X += accel * 2f;
            }
            else if (NPC.velocity.X > desiredVelocity.X)
            {
                NPC.velocity.X -= accel;
                if (NPC.velocity.X > 0f && desiredVelocity.X < 0f)
                    NPC.velocity.X -= accel * 2f;
            }

            if (NPC.velocity.Y < desiredVelocity.Y)
            {
                NPC.velocity.Y += accel;
                if (NPC.velocity.Y < 0f && desiredVelocity.Y > 0f)
                    NPC.velocity.Y += accel * 2f;
            }
            else if (NPC.velocity.Y > desiredVelocity.Y)
            {
                NPC.velocity.Y -= accel;
                if (NPC.velocity.Y > 0f && desiredVelocity.Y < 0f)
                    NPC.velocity.Y -= accel * 2f;
            }

            // ===== 5) 头部始终朝向玩家 =====
            Vector2 look = player.Center - NPC.Center;
            NPC.rotation = look.ToRotation() + MathHelper.Pi;

            // 你的头部原图默认朝左：
            // 玩家在右边 -> 需要水平翻转
            NPC.spriteDirection = player.Center.X > NPC.Center.X ? 1 : -1;
            NPC.direction = NPC.spriteDirection;
        }
        private void UpdateAnchorVisuals()
        {
            for (int i = 0; i < TentacleCount; i++)
            {
                if (!_anchorValid[i])
                {
                    _anchorDrawWorld[i] = Vector2.Lerp(_anchorDrawWorld[i], NPC.Center, 0.2f);
                    _anchorMoving[i] = false;
                    continue;
                }

                // 初始化，避免首次出现时从 (0,0) 飞过来
                if (_anchorDrawWorld[i] == Vector2.Zero)
                {
                    _anchorDrawWorld[i] = _anchorWorld[i];
                    _anchorMoveTarget[i] = _anchorWorld[i];
                    _anchorMoving[i] = false;
                }

                if (_anchorMoving[i])
                {
                    Vector2 toTarget = _anchorMoveTarget[i] - _anchorDrawWorld[i];
                    float dist = toTarget.Length();

                    if (dist <= _anchorMoveSpeed[i] || dist < 6f)
                    {
                        _anchorDrawWorld[i] = _anchorMoveTarget[i];
                        _anchorMoving[i] = false;
                    }
                    else
                    {
                        _anchorDrawWorld[i] += toTarget / dist * _anchorMoveSpeed[i];
                    }
                }
                else
                {
                    // 抓住目标后轻微贴近，避免浮点误差
                    _anchorDrawWorld[i] = Vector2.Lerp(_anchorDrawWorld[i], _anchorWorld[i], 0.18f);
                }
            }
        }
        // 只要任意一根正在移动，本帧就不允许任何其他触手开始移动
        // 当没有触手在移动时，从“需要换点的触手”里，选离玩家最远的那一根
        private void UpdateAnchors(Player player)
        {
            // 只要有一根在移动，本帧就完全不启动新的移动
            if (AnyAnchorMoving())
            {
                return;
            }

            int chosenIndex = -1;
            float farthestDist = -1f;

            // 先找“本帧需要换点”的触手里，离玩家最远的那一根
            for (int i = 0; i < TentacleCount; i++)
            {
                _anchorTimer[i]++;

                bool needNewAnchor = false;

                if (!_anchorValid[i])
                {
                    needNewAnchor = true;
                }
                else
                {
                    Point p = _anchorTiles[i];

                    if (!IsValidAnchorTile(p.X, p.Y))
                        needNewAnchor = true;

                    if (Vector2.Distance(_anchorWorld[i], NPC.Center) > MaxVineLength)
                        needNewAnchor = true;

                    // 定时主动换点，避免一直不伸新触手
                    if (_anchorTimer[i] >= 180)
                    {
                        _anchorTimer[i] = 0;
                        if (Main.rand.NextBool(5))
                            needNewAnchor = true;
                    }
                }

                if (!needNewAnchor)
                    continue;

                float distToPlayer = Vector2.Distance(_anchorWorld[i], player.Center);
                if (!_anchorValid[i])
                    distToPlayer = 999999f; // 无效锚点优先级最高

                if (distToPlayer > farthestDist)
                {
                    farthestDist = distToPlayer;
                    chosenIndex = i;
                }
            }

            // 本帧没有任何需要换点的触手
            if (chosenIndex == -1)
                return;

            int slot = chosenIndex;

            if (TryFindAnchor(player, slot, out Point found))
            {
                Vector2 newWorld = TileCenter(found.X, found.Y);

                // 首次抓取：直接放过去
                if (!_anchorValid[slot] || _anchorDrawWorld[slot] == Vector2.Zero)
                {
                    _anchorDrawWorld[slot] = newWorld;
                    _anchorMoving[slot] = false;
                }
                else
                {
                    // 这里只允许这一根启动移动
                    _anchorMoveTarget[slot] = newWorld;
                    _anchorMoving[slot] = true;
                    _anchorMoveSpeed[slot] = 10f + Main.rand.NextFloat(2f, 5f);
                }

                _anchorTiles[slot] = found;
                _anchorWorld[slot] = newWorld;
                _anchorValid[slot] = true;
                NPC.netUpdate = true;
            }
            else
            {
                _anchorValid[slot] = false;
                _anchorWorld[slot] = NPC.Center;
                NPC.netUpdate = true;
            }
        }

        private bool TryFindAnchor(Player player, int slot, out Point result)
        {
            result = Point.Zero;

            // 三根触手尽量分区域抓块，更像世纪之花
            Vector2 bias;
            switch (slot)
            {
                default:
                case 0:
                    bias = new Vector2(-160f, -120f);
                    break;
                case 1:
                    bias = new Vector2(0f, -180f);
                    break;
                case 2:
                    bias = new Vector2(160f, -120f);
                    break;
            }

            Vector2 baseWorld = player.Center + bias;

            // 第一轮：优先在玩家附近的指定区域找
            for (int i = 0; i < 90; i++)
            {
                Vector2 sample = baseWorld + Main.rand.NextVector2Circular(120f, 100f);
                Point tile = sample.ToTileCoordinates();

                if (IsGoodAnchorForHead(tile.X, tile.Y) && IsAnchorFarEnoughFromOthers(tile, slot))
                {
                    result = tile;
                    return true;
                }
            }

            // 第二轮：头部附近兜底
            Point center = NPC.Center.ToTileCoordinates();
            for (int x = center.X - 28; x <= center.X + 28; x += 2)
            {
                for (int y = center.Y - 28; y <= center.Y + 28; y += 2)
                {
                    if (IsGoodAnchorForHead(x, y) && IsAnchorFarEnoughFromOthers(new Point(x, y), slot))
                    {
                        result = new Point(x, y);
                        return true;
                    }
                }
            }

            return false;
        }
        private bool IsAnchorFarEnoughFromOthers(Point candidate, int slot)
        {
            Vector2 candidateWorld = TileCenter(candidate.X, candidate.Y);

            for (int i = 0; i < TentacleCount; i++)
            {
                if (i == slot || !_anchorValid[i])
                    continue;

                float minSpacing = 96f; // 6格，够明显分开
                if (Vector2.Distance(candidateWorld, _anchorWorld[i]) < minSpacing)
                    return false;
            }

            return true;
        }
        private bool IsGoodAnchorForHead(int x, int y)
        {
            if (!WorldGen.InWorld(x, y, 10))
                return false;

            if (!IsValidAnchorTile(x, y))
                return false;

            Vector2 world = TileCenter(x, y);
            if (Vector2.Distance(world, NPC.Center) > MaxVineLength)
                return false;

            return true;
        }

        private bool IsValidAnchorTile(int x, int y)
        {
            if (!WorldGen.InWorld(x, y, 10))
                return false;

            Tile tile = Main.tile[x, y];
            if (tile == null)
                return false;

            // 方案1：有实心块就能抓（放宽，不要求暴露）
            bool solidAnchor =
                tile.HasTile &&
                Main.tileSolid[tile.TileType] &&
                !Main.tileSolidTop[tile.TileType] &&
                !tile.IsActuated;

            // 方案2：有墙也能抓
            bool wallAnchor = tile.WallType > WallID.None;

            return solidAnchor || wallAnchor;
        }

        private static Vector2 TileCenter(int x, int y)
        {
            return new Vector2(x * 16 + 8, y * 16 + 8);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;

            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y >= frameHeight * 3)
                    NPC.frame.Y = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            DrawVines(spriteBatch, screenPos, drawColor);
            DrawTentacleHeads(spriteBatch, screenPos, drawColor);
            DrawMainHead(spriteBatch, screenPos, drawColor);
            return false;
        }

        private void DrawVines(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D vine = TextureAssets.Chain27.Value;

            for (int i = 0; i < TentacleCount; i++)
            {
                if (!_anchorValid[i])
                    continue;

                Vector2 start = _anchorDrawWorld[i];
                Vector2 end = NPC.Center;
                Vector2 diff = end - start;
                float length = diff.Length();

                if (length <= 4f)
                    continue;

                diff /= length;
                float rotation = diff.ToRotation() - MathHelper.PiOver2;

                for (float j = 0; j <= length; j += vine.Height)
                {
                    Vector2 drawPos = start + diff * j;

                    spriteBatch.Draw(
                        vine,
                        drawPos - screenPos,
                        null,
                        drawColor,
                        rotation,
                        vine.Size() * 0.5f,
                        1f,
                        SpriteEffects.None,
                        0f
                    );
                }
            }
        }

        private void DrawTentacleHeads(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(TentacleTexturePath).Value;

            int frameHeight = texture.Height / 4;
            int currentFrame = (int)(Main.GameUpdateCount / 6 % 4);

            Rectangle frame = new Rectangle(0, currentFrame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);

            for (int i = 0; i < TentacleCount; i++)
            {
                if (!_anchorValid[i])
                    continue;

                Vector2 toHead = _anchorDrawWorld[i] - NPC.Center;

                // 你要求“触手方向与藤蔓延伸方向相反”
                float rotation = toHead.ToRotation() + MathHelper.Pi;

                // SpriteEffects fx = toHead.X > 0f ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                spriteBatch.Draw(
                    texture,
                    _anchorDrawWorld[i] - screenPos,
                    frame,
                    drawColor,
                    rotation,
                    origin,
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        private void DrawMainHead(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Rectangle frame = NPC.frame;

            // Vector2 toPlayer = NPC.Center - NPC.targetPosition;

            // 你要求“触手方向与藤蔓延伸方向相反”
            // float rotation = toPlayer.ToRotation() + MathHelper.Pi;

            Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);
            // 把贴图往左挪，制造“碰撞箱更靠右”的视觉效果
            Vector2 drawOffset = new Vector2(-15f * NPC.direction, 0f);
            spriteBatch.Draw(
                texture,
                NPC.Center - screenPos + drawOffset,
                frame,
                drawColor,
                NPC.rotation,
                origin,
                1.1f,// 头部缩放注意匹配碰撞箱
                SpriteEffects.None,
                0f
            );
        }
        private bool AnyAnchorMoving()
        {
            for (int i = 0; i < TentacleCount; i++)
            {
                if (_anchorMoving[i])
                    return true;
            }

            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            for (int i = 0; i < TentacleCount; i++)
            {
                writer.Write(_anchorValid[i]);
                writer.Write(_anchorTiles[i].X);
                writer.Write(_anchorTiles[i].Y);
                writer.Write(_anchorTimer[i]);
                writer.WriteVector2(_anchorDrawWorld[i]);
                writer.WriteVector2(_anchorMoveTarget[i]);
                writer.Write(_anchorMoving[i]);
                writer.Write(_anchorMoveSpeed[i]);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            for (int i = 0; i < TentacleCount; i++)
            {
                _anchorValid[i] = reader.ReadBoolean();

                int x = reader.ReadInt32();
                int y = reader.ReadInt32();
                _anchorTiles[i] = new Point(x, y);

                _anchorWorld[i] = TileCenter(x, y);
                _anchorTimer[i] = reader.ReadInt32();
                _anchorDrawWorld[i] = reader.ReadVector2();
                _anchorMoveTarget[i] = reader.ReadVector2();
                _anchorMoving[i] = reader.ReadBoolean();
                _anchorMoveSpeed[i] = reader.ReadSingle();
            }
        }
    }
}