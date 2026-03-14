using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items;

namespace WuDao.Content.Global.NPCs
{
    public class GreenWorm : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;

            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
            NPCID.Sets.TownCritter[Type] = false;
        }

        public override void SetDefaults()
        {
            // 直接模仿原版蠕虫
            NPC.CloneDefaults(NPCID.Worm);

            AIType = NPCID.Worm;
            AnimationType = NPCID.Worm;

            NPC.width = 14;
            NPC.height = 16;
            NPC.catchItem = (short)ModContent.ItemType<DuckMountItem>();

            NPC.dontCountMe = true; // 作为小动物，不计入敌怪数量
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // 不在这里单独写概率，改由 GlobalNPC 从原版 Worm 权重里分流
            return 0f;
        }
    }
}