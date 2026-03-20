using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 春秋蝉buff
    public class RewinderCicadasBuff : ModBuff
    {
        public override string Texture => $"WuDao/Content/Items/Accessories/RewinderCicadas";
        private static Asset<Texture2D> RewinderCicadasTexture;
        public override void SetStaticDefaults()
        {
            DisplayName.Format("Temporal Exhaustion");
            Description.Format("you can't use it in cool down state");
            Main.debuff[Type] = true;        // 作为负面状态显示
            Main.buffNoSave[Type] = false;   // 存档（随存档记忆剩余时间）
            Main.buffNoTimeDisplay[Type] = false; // 显示剩余时间
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;

            RewinderCicadasTexture = ModContent.Request<Texture2D>("Terraria/Images/Buff");
        }
        public override void Unload()
        {
            RewinderCicadasTexture = null;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
        {
            if (Main.dedServ || RewinderCicadasTexture == null)
            {
                return false;
            }
            // 先画 buff 底图
            Texture2D texture = RewinderCicadasTexture.Value;

            Rectangle source = new Rectangle(0, 0, 32, 32);
            Vector2 origin = new Vector2(0, 0);

            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                source,
                drawParams.DrawColor, // 不要再用 Color.White
                0f,
                origin,
                1,
                SpriteEffects.None,
                0
            );

            return true;
        }
    }
}
