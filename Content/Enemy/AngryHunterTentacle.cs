using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace WuDao.Content.Enemy
{
    public class AngryHunterTentacle : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 24;
            NPC.height = 24;

            NPC.damage = 24;
            NPC.defense = 0;
            NPC.lifeMax = 80;

            NPC.knockBackResist = 0f;

            NPC.aiStyle = -1;

            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }

        public override void AI()
        {
            int head = (int)NPC.ai[0];

            if (!Main.npc[head].active)
            {
                NPC.active = false;
                return;
            }

            NPC headNPC = Main.npc[head];

            float angle = NPC.ai[1] * MathHelper.TwoPi / 4f;

            float distance = 70f;

            Vector2 offset =
                new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle))
                * distance;

            NPC.Center = headNPC.Center + offset;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;

            if (NPC.frameCounter > 6)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;

                if (NPC.frame.Y >= frameHeight * 4)
                    NPC.frame.Y = 0;
            }
        }


        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            NPC head = Main.npc[(int)NPC.ai[0]];

            Texture2D vine = TextureAssets.Chain27.Value;

            Vector2 position = NPC.Center;
            Vector2 mountedCenter = head.Center;

            Vector2 dist = mountedCenter - position;

            float length = dist.Length();

            dist.Normalize();

            float rotation = dist.ToRotation() - MathHelper.PiOver2;

            for (float i = 0; i <= length; i += vine.Height)
            {
                Vector2 drawPos = position + dist * i;

                spriteBatch.Draw(
                    vine,
                    drawPos - screenPos,
                    null,
                    drawColor,
                    rotation,
                    vine.Size() / 2,
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }

            return true;
        }
    }
}