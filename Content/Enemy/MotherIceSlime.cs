using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Enemy
{
    public class MotherIceSlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4; // 4帧循环
        }

        public override void SetDefaults()
        {
            NPC.width = 74;
            NPC.height = 52;

            NPC.damage = 180;
            NPC.defense = 26;
            NPC.lifeMax = 600;

            NPC.knockBackResist = 0.2f;
            NPC.value = 10f;

            NPC.aiStyle = NPCAIStyleID.Slime;              // 最简单史莱姆AI
            AIType = NPCID.BlueSlime;     // 继承蓝史莱姆行为（跳一跳接近玩家）
            AnimationType = -1;           // 我们自己写4帧循环

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }

        public override void FindFrame(int frameHeight)
        {
            // 不管在地面/空中都循环播放（你要求“不断循环即可”）
            NPC.frameCounter++;
            if (NPC.frameCounter >= 6) // 数值越小动画越快
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * 4)
                    NPC.frame.Y = 0;
            }
        }
        public override void OnKill()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int babyType = NPCID.IceSlime;

            // 让4个宝宝在尸体附近“十字分布”，避免重叠
            Vector2[] offsets =
            {
                new Vector2(-22f, 0f),
                new Vector2( 22f, 0f),
                new Vector2( 0f, -18f),
                new Vector2( 0f, 18f),
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2 spawnPos = NPC.Center + offsets[i];

                int idx = NPC.NewNPC(
                    NPC.GetSource_Death(),
                    (int)spawnPos.X,
                    (int)spawnPos.Y,
                    babyType
                );

                if (idx >= 0 && idx < Main.maxNPCs)
                {
                    NPC baby = Main.npc[idx];

                    // 给一点初速度，把他们“弹开”，更不容易重叠
                    baby.velocity = offsets[i].SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(2.2f, 3.4f);
                    baby.velocity.Y -= Main.rand.NextFloat(1.0f, 2.0f);

                    baby.netUpdate = true;
                }
            }
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // 冰雪环境
            if (spawnInfo.Player.ZoneSnow && Main.hardMode)
            {
                return 0.25f;
            }
            return 0f;
        }
    }
}