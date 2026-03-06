using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
namespace WuDao.Content.Enemy
{
    public class AngryHunterTentacle : ModNPC
    {
        private const float MaxVineLength = AngryHunterHead.MaxVineLength;
        private const int SearchInterval = 24; // 每隔24帧尝试检查/换锚点
        public override bool CheckActive() => false;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 29;
            NPC.height = 29;

            // 如果你希望只有头部造成伤害，这里就设为0
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 9999;
            NPC.dontTakeDamage = true;
            NPC.knockBackResist = 0f;

            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.chaseable = false;
        }

        public override void AI()
        {
            int headIndex = (int)NPC.ai[0];
            if (headIndex < 0 || headIndex >= Main.maxNPCs)
            {
                NPC.active = false;
                return;
            }

            NPC head = Main.npc[headIndex];
            if (!head.active || head.type != ModContent.NPCType<AngryHunterHead>())
            {
                NPC.active = false;
                return;
            }

            Player target = Main.player[head.target];
            if (!target.active || target.dead)
            {
                NPC.active = false;
                return;
            }

            NPC.realLife = head.whoAmI;

            // ai[2], ai[3] 存锚点 tile 坐标
            Point anchorTile = new Point((int)NPC.ai[2], (int)NPC.ai[3]);

            bool needNewAnchor = false;

            if (anchorTile == Point.Zero)
                needNewAnchor = true;

            if (!needNewAnchor && !IsValidAnchorTile(anchorTile.X, anchorTile.Y))
                needNewAnchor = true;

            Vector2 anchorWorld = TileCenter(anchorTile.X, anchorTile.Y);
            if (!needNewAnchor && Vector2.Distance(anchorWorld, head.Center) > MaxVineLength)
                needNewAnchor = true;

            NPC.localAI[0]++;
            if (NPC.localAI[0] >= SearchInterval)
            {
                NPC.localAI[0] = 0f;

                // 偶尔重选，让它更像会不断重新钩住环境
                if (Main.rand.NextBool(5))
                    needNewAnchor = true;
            }

            if (needNewAnchor)
            {
                if (TryFindAnchor(head, target, out Point found))
                {
                    NPC.ai[2] = found.X;
                    NPC.ai[3] = found.Y;
                    anchorTile = found;
                    anchorWorld = TileCenter(found.X, found.Y);
                    NPC.netUpdate = true;
                }
                else
                {
                    // 找不到锚点时，先临时挂在头部附近，避免消失/乱飞
                    Vector2 idlePos = head.Center + new Vector2(0f, 28f).RotatedBy(MathHelper.TwoPi / AngryHunterHead.TentacleCount * NPC.ai[1]);
                    NPC.Center = Vector2.Lerp(NPC.Center, idlePos, 0.2f);
                    NPC.velocity = Vector2.Zero;
                    UpdateVisual(head);
                    return;
                }
            }

            // 固定在锚点上
            NPC.Center = Vector2.Lerp(NPC.Center, anchorWorld, 0.35f);
            if (Vector2.Distance(NPC.Center, anchorWorld) < 2f)
                NPC.Center = anchorWorld;

            NPC.velocity = Vector2.Zero;

            UpdateVisual(head);
        }

        private void UpdateVisual(NPC head)
        {
            Vector2 toHead = head.Center - NPC.Center;
            if (toHead != Vector2.Zero)
                NPC.rotation = toHead.ToRotation() + MathHelper.PiOver2;

            NPC.spriteDirection = toHead.X >= 0f ? 1 : -1;
        }

        private static Vector2 TileCenter(int x, int y)
        {
            return new Vector2(x * 16 + 8, y * 16 + 8);
        }

        private bool TryFindAnchor(NPC head, Player target, out Point result)
        {
            result = Point.Zero;

            Vector2 headTile = head.Center / 16f;
            Vector2 playerTile = target.Center / 16f;

            Vector2 forward = (target.Center - head.Center).SafeNormalize(Vector2.UnitY);
            Vector2 side = forward.RotatedBy(MathHelper.PiOver2);

            // 第一轮：优先在“头->玩家”方向附近找
            for (int i = 0; i < 70; i++)
            {
                float forwardDist = MathHelper.Lerp(10f, 55f, Main.rand.NextFloat());
                float sideDist = Main.rand.NextFloat(-18f, 18f);

                Vector2 sample = headTile + forward * forwardDist + side * sideDist;
                Point p = sample.ToPoint();

                if (IsGoodAnchorForHead(p.X, p.Y, head))
                {
                    result = p;
                    return true;
                }
            }

            // 第二轮：在玩家周围环形采样
            for (int i = 0; i < 70; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(24f, 24f);
                Point p = (playerTile + offset).ToPoint();

                if (IsGoodAnchorForHead(p.X, p.Y, head))
                {
                    result = p;
                    return true;
                }
            }

            // 第三轮：兜底，扫描头周围一个矩形区域
            Point center = head.Center.ToTileCoordinates();
            for (int x = center.X - 30; x <= center.X + 30; x += 3)
            {
                for (int y = center.Y - 30; y <= center.Y + 30; y += 3)
                {
                    if (IsGoodAnchorForHead(x, y, head))
                    {
                        result = new Point(x, y);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsGoodAnchorForHead(int x, int y, NPC head)
        {
            if (!WorldGen.InWorld(x, y, 10))
                return false;

            if (!IsValidAnchorTile(x, y))
                return false;

            Vector2 world = TileCenter(x, y);
            if (Vector2.Distance(world, head.Center) > MaxVineLength)
                return false;

            return true;
        }

        private bool IsValidAnchorTile(int x, int y)
        {
            if (!WorldGen.InWorld(x, y, 10))
                return false;

            Tile tile = Main.tile[x, y];
            if (tile == null || !tile.HasTile)
                return false;

            if (!Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
                return false;

            if (tile.IsActuated)
                return false;

            // 至少有一个相邻方向是空气/非实心，避免挂在完全埋住的块里
            bool exposed =
                !IsSolidTile(x + 1, y) ||
                !IsSolidTile(x - 1, y) ||
                !IsSolidTile(x, y + 1) ||
                !IsSolidTile(x, y - 1);

            return exposed;
        }

        private bool IsSolidTile(int x, int y)
        {
            if (!WorldGen.InWorld(x, y, 10))
                return false;

            Tile tile = Main.tile[x, y];
            return tile != null && tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && !tile.IsActuated;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;

            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y >= frameHeight * 4)
                    NPC.frame.Y = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            int headIndex = (int)NPC.ai[0];
            if (headIndex < 0 || headIndex >= Main.maxNPCs)
                return true;

            NPC head = Main.npc[headIndex];
            if (!head.active)
                return true;

            Texture2D vine = TextureAssets.Chain27.Value;

            Vector2 start = NPC.Center;
            Vector2 end = head.Center;
            Vector2 diff = end - start;

            float length = diff.Length();
            if (length > 4f)
            {
                diff /= length;
                float rotation = diff.ToRotation() - MathHelper.PiOver2;

                for (float i = 0; i <= length; i += vine.Height)
                {
                    Vector2 drawPos = start + diff * i;

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

            return true;
        }
    }
}