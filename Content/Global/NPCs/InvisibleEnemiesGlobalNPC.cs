using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using WuDao.Common;
using WuDao.Content.Systems;

namespace WuDao.Content.Global.NPCs
{
    public class InvisibleEnemiesGlobalNPC : GlobalNPC
    {
        private static bool ShouldHide(NPC npc)
        {
            if (!npc.active || npc.friendly || npc.townNPC)
                return false;

            Player viewer = Main.LocalPlayer;
            return !InvisibleEnemies.CanSeeEcho(viewer);
        }

        public override void PostAI(NPC npc)
        {
            if (!ShouldHide(npc))
                return;

            // 1. 当前/上一帧扫掠
            Rectangle currentSweep = Rectangle.Union(
                new Rectangle((int)npc.oldPosition.X, (int)npc.oldPosition.Y, npc.width, npc.height),
                npc.Hitbox
            );

            int currentExtra = 24 + (int)npc.velocity.Length() * 2;
            currentSweep.Inflate(currentExtra, currentExtra);
            InvisibleEnemiesDustSystem.AddHiddenNPCZone(currentSweep);

            // 2. 多帧轨迹带：覆盖冲刺残影/旧位置产尘
            int historyCount = npc.oldPos.Length;
            for (int i = 0; i < historyCount; i++)
            {
                Vector2 oldPos = npc.oldPos[i];
                if (oldPos == Vector2.Zero)
                    continue;

                Rectangle oldBox = new Rectangle((int)oldPos.X, (int)oldPos.Y, npc.width, npc.height);

                int extra = 36 + i * 8;
                oldBox.Inflate(extra, extra);

                InvisibleEnemiesDustSystem.AddHiddenNPCZone(oldBox);
            }

            // 3. 保留一个较大的前方区域，处理冲刺头部/前缘特效
            Vector2 forwardCenter = npc.Center + npc.velocity * 0.5f;
            Rectangle forwardZone = Utils.CenteredRectangle(forwardCenter, new Vector2(npc.width + 120f, npc.height + 120f));
            InvisibleEnemiesDustSystem.AddHiddenNPCZone(forwardZone);
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ShouldHide(npc))
                return false;

            return true;
        }
    }
}
