using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace WuDao.Common.GlobalNPCs
{
    // 在敌怪头顶绘制 buff 图标
    public class NPCBuffDisplay : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!npc.active || npc.life <= 0)
                return;

            const int iconSize = 20; // Buff 图标大小
            int buffCount = 0;

            // 先计算有多少 Buff，要居中绘制
            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                if (npc.buffType[i] > 0)
                    buffCount++;
            }

            if (buffCount == 0)
                return;

            Vector2 drawStart = npc.Top - screenPos + new Vector2(-(buffCount * iconSize) / 2f, -40);

            int drawn = 0;
            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                int buffType = npc.buffType[i];
                if (buffType <= 0)
                    continue;

                Texture2D texture = TextureAssets.Buff[buffType].Value;
                Vector2 drawPos = drawStart + new Vector2(drawn * iconSize, 0);
                Rectangle sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);

                spriteBatch.Draw(texture, drawPos, sourceRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                drawn++;
            }
        }
    }
}
