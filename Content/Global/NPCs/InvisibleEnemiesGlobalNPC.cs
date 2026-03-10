using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using WuDao.Common;
using WuDao.Content.Systems;
using Terraria.ID;

namespace WuDao.Content.Global.NPCs
{
    public class InvisibleEnemiesGlobalNPC : GlobalNPC
    {
        private static NPC GetHideOwner(NPC npc)
        {
            if (!npc.active)
                return npc;

            // 对多段体 / 子体 / Boss 部件，优先跟随 realLife 主体
            if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs)
            {
                NPC owner = Main.npc[npc.realLife];
                if (owner.active)
                    return owner;
            }

            return npc;
        }

        // private static bool ShouldHide(NPC npc)
        // {
        //     if (!npc.active)
        //         return false;

        //     Player viewer = Main.LocalPlayer;

        //     NPC owner = GetHideOwner(npc);

        //     if (!owner.active || owner.friendly || owner.townNPC)
        //         return false;

        //     return !InvisibleEnemies.CanSeeEcho(viewer);
        // }
        internal static bool ShouldHideForDraw(NPC npc)
        {
            if (!npc.active)
                return false;

            Player viewer = Main.LocalPlayer;
            if (viewer == null || !viewer.active)
                return false;

            NPC owner = GetHideOwner(npc);

            if (!owner.active || owner.friendly || owner.townNPC)
                return false;

            return !InvisibleEnemies.CanSeeEcho(viewer);
        }
        public override void PostAI(NPC npc)
        {
            if (!ShouldHideForDraw(npc))
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
            if (ShouldHideForDraw(npc))
                return false;

            return true;
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!ShouldHideForDraw(npc))
                return;

            drawColor = Color.Transparent;
        }

        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            if (ShouldHideForDraw(npc))
                return Color.Transparent;

            return null;
        }
    }
}
