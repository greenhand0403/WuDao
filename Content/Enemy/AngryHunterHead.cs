using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace WuDao.Content.Enemy
{
    public class AngryHunterHead : ModNPC
    {
        public const int TentacleCount = 3;
        public const float MaxVineLength = 55f * 16f; // 55格
        public const float DesiredVineLength = 240f;  // 理想藤蔓长度，可自行调
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 3;
        }

        public override void SetDefaults()
        {
            NPC.width = 50;
            NPC.height = 50;

            NPC.damage = 34;
            NPC.defense = 10;
            NPC.lifeMax = 220;

            NPC.knockBackResist = 0.45f;

            NPC.aiStyle = -1;

            NPC.value = Item.buyPrice(0, 0, 2, 0);

            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }

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

            SpawnTentaclesOnce();

            Vector2 toPlayer = player.Center - NPC.Center;
            float distanceToPlayer = toPlayer.Length();

            Vector2 vinePull = Vector2.Zero;
            int anchoredCount = 0;

            for (int i = 0; i < TentacleCount; i++)
            {
                int tentacleIndex = GetTentacleIndex(i);
                if (tentacleIndex < 0)
                    continue;

                NPC tentacle = Main.npc[tentacleIndex];
                if (!tentacle.active || tentacle.type != ModContent.NPCType<AngryHunterTentacle>())
                    continue;

                anchoredCount++;

                Vector2 toAnchor = tentacle.Center - NPC.Center;
                float currentLength = toAnchor.Length();
                if (currentLength < 8f)
                    continue;

                Vector2 dir = toAnchor / currentLength;

                // 超过理想长度时，藤蔓明显把头拉过去
                if (currentLength > DesiredVineLength)
                {
                    float stretch = currentLength - DesiredVineLength;
                    float pullStrength = MathHelper.Clamp(stretch / 180f, 0f, 1.25f);

                    vinePull += dir * (0.18f + pullStrength * 0.42f);
                }
                else
                {
                    // 太短时给一点轻微回弹，避免一直贴在锚点边上抖
                    float slack = DesiredVineLength - currentLength;
                    float pushStrength = MathHelper.Clamp(slack / 220f, 0f, 0.15f);
                    vinePull -= dir * pushStrength * 0.08f;
                }
            }

            // 基础朝玩家偏置：触手越多，越靠藤蔓拉；触手少时，直接追玩家稍微强一点
            Vector2 chaseDir = toPlayer.SafeNormalize(Vector2.Zero);
            float chaseStrength = anchoredCount >= 2 ? 0.14f : 0.26f;

            // 太靠近玩家时减少直冲，略微环绕
            Vector2 orbit = Vector2.Zero;
            if (distanceToPlayer < 120f)
            {
                orbit = chaseDir.RotatedBy(MathHelper.PiOver2 * NPC.direction) * 0.08f;
                chaseStrength *= 0.45f;
            }

            NPC.velocity *= 0.965f;
            NPC.velocity += vinePull;
            NPC.velocity += chaseDir * chaseStrength;
            NPC.velocity += orbit;

            float maxSpeed = anchoredCount >= 2 ? 7.8f : 5.8f;
            if (NPC.velocity.Length() > maxSpeed)
                NPC.velocity = Vector2.Normalize(NPC.velocity) * maxSpeed;

            // 朝向：你的头部贴图本来就是朝左，所以直接跟 direction 即可
            if (NPC.velocity.X != 0f)
                NPC.direction = NPC.velocity.X > 0f ? 1 : -1;

            NPC.spriteDirection = NPC.direction;

            // 轻微转向感
            NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.045f, -0.35f, 0.35f);

            // 防止离玩家太远时卡住
            if (distanceToPlayer > 1100f)
            {
                NPC.velocity += chaseDir * 0.18f;
            }
        }

        private void SpawnTentaclesOnce()
        {
            if (NPC.localAI[3] == 1f)
                return;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < TentacleCount; i++)
                {
                    int id = NPC.NewNPC(
                        NPC.GetSource_FromAI(),
                        (int)NPC.Center.X,
                        (int)NPC.Center.Y,
                        ModContent.NPCType<AngryHunterTentacle>(),
                        ai0: NPC.whoAmI,
                        ai1: i
                    );

                    if (id >= 0 && id < Main.maxNPCs)
                    {
                        Main.npc[id].realLife = NPC.whoAmI;
                        NPC.localAI[i] = id + 1; // 0 作为“没生成”
                    }
                }

                NPC.netUpdate = true;
            }

            NPC.localAI[3] = 1f;
        }

        private int GetTentacleIndex(int slot)
        {
            if (slot < 0 || slot >= TentacleCount)
                return -1;

            int stored = (int)NPC.localAI[slot];
            if (stored <= 0)
                return -1;

            return stored - 1;
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
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Rectangle frame = NPC.frame;

            SpriteEffects effects = NPC.spriteDirection == 1
                ? SpriteEffects.None
                : SpriteEffects.FlipHorizontally;

            // 头图单帧100x56，命中箱比贴图小，所以要手动画 整体视觉往右移
            Vector2 origin = new Vector2(frame.Width / 2f + 25f * NPC.direction, frame.Height / 2f);

            spriteBatch.Draw(
                texture,
                NPC.Center - screenPos,
                frame,
                drawColor,
                NPC.rotation,
                origin,
                1f,
                effects,
                0f
            );

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
                return;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < TentacleCount; i++)
                {
                    int tentacleIndex = GetTentacleIndex(i);
                    if (tentacleIndex >= 0 && Main.npc[tentacleIndex].active)
                    {
                        Main.npc[tentacleIndex].life = 0;
                        Main.npc[tentacleIndex].HitEffect();
                        Main.npc[tentacleIndex].checkDead();
                    }
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneJungle)
                return 0.12f;

            return 0f;
        }
    }
}