using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using WuDao.Common;

namespace WuDao.Content.Global.NPCs
{
    public class InvisibleEnemiesGlobalNPC : GlobalNPC
    {
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // 只影响敌怪（你也可以排除城镇NPC、雕像刷怪等）
            if (!npc.friendly && npc.active)
            {
                var viewer = Main.LocalPlayer;
                if (!InvisibleEnemies.CanSeeEcho(viewer))
                {
                    return false; // 不画 => “隐身”
                }
            }

            return true;
        }
    }
}
