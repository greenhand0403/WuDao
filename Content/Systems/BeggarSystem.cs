using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using WuDao.Content.Global.NPCs;

namespace WuDao.Content.Systems
{
    // 绝学NPC：流浪乞丐
    public class BeggarSystem : ModSystem
    {
        private bool lastDay = true;

        public override void PostUpdateWorld()
        {
            // 早晨切换检测
            if (Main.dayTime && !lastDay)
            {
                OnNewDay();
            }
            lastDay = Main.dayTime;
        }

        private void OnNewDay()
        {
            // 25% 几率在白天到来时生成一个流浪乞丐
            if (Main.rand.NextFloat() < 0.25f)
            {
                SpawnBeggarNear(Main.LocalPlayer);
            }
        }

        public static void SpawnBeggarNear(Player p)
        {
            if (p == null || !p.active) return;
            // 如果已经存在则不再生成
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var n = Main.npc[i];
                if (n.active && n.type == ModContent.NPCType<WanderingBeggar>()) return;
            }
            var pos = p.Center + new Vector2(120f * (p.direction == 0 ? 1 : p.direction), -16);
            NPC.NewNPC(p.GetSource_Misc("BaoziSummon"), (int)pos.X, (int)pos.Y, ModContent.NPCType<WanderingBeggar>());
        }
    }
}
