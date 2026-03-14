using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Global.NPCs
{
    public class GreenWormSpawnGlobalNPC : GlobalNPC
    {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            int wormID = NPCID.Worm;
            int greenWormID = ModContent.NPCType<GreenWorm>();

            if (pool.TryGetValue(wormID, out float wormWeight) && wormWeight > 0f)
            {
                // 从原版 Worm 的权重中分出 6%
                float greenWeight = wormWeight * 0.06f;
                pool[wormID] = wormWeight * 0.94f;

                if (pool.ContainsKey(greenWormID))
                    pool[greenWormID] += greenWeight;
                else
                    pool[greenWormID] = greenWeight;
            }
        }
    }
}