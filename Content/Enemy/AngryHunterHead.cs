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
        const int TentacleCount = 4;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 3;
        }

        public override void SetDefaults()
        {
            NPC.width = 50;
            NPC.height = 50;

            NPC.damage = 32;
            NPC.defense = 8;
            NPC.lifeMax = 180;

            NPC.knockBackResist = 0.6f;

            NPC.aiStyle = -1;

            NPC.value = Item.buyPrice(0, 0, 2, 0);

            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];

            NPC.TargetClosest();

            Vector2 move = player.Center - NPC.Center;
            float speed = 4f;

            NPC.velocity = Vector2.Lerp(NPC.velocity, move.SafeNormalize(Vector2.Zero) * speed, 0.05f);

            //生成触手
            if (NPC.localAI[0] == 0)
            {
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
                    }
                }

                NPC.localAI[0] = 1;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;

            if (NPC.frameCounter > 6)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;

                if (NPC.frame.Y >= frameHeight * 3)
                    NPC.frame.Y = 0;
            }
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneJungle)
                return 0.15f;

            return 0f;
        }
    }
}