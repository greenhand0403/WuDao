using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items.Accessories;

namespace WuDao.Content.Enemy
{
    public class BloodySlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4; // 4帧循环
        }

        public override void SetDefaults()
        {
            NPC.width = 74;
            NPC.height = 52;
            NPC.scale = 0.6f;

            NPC.damage = 60;
            NPC.defense = 10;
            NPC.lifeMax = 250;

            NPC.knockBackResist = 0.7f;
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
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneCrimson && !Main.hardMode)
            {
                return 0.25f;
            }
            return 0f;
        }
        // 5%掉落猩红立方
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CrimPowerCube>(), 100));
        }
    }
}