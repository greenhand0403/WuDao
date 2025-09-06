using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace WuDao.Content.UI
{
    public static class QiBarDrawer
    {
        public static void DrawSimpleBar(Rectangle rect, float value, int max, string label)
        {
            var sb = Main.spriteBatch;
            var bg = new Color(0, 0, 0, 160);
            var fill = new Color(80, 200, 255, 220);
            var border = new Color(255, 255, 255, 100);

            // 背景
            sb.Draw(TextureAssets.MagicPixel.Value, rect, bg);

            // 内部填充
            float pct = max > 0 ? value / max : 0f;
            pct = MathHelper.Clamp(pct, 0f, 1f);
            var fillRect = new Rectangle(rect.X + 2, rect.Y + 2, (int)((rect.Width - 4) * pct), rect.Height - 4);
            sb.Draw(TextureAssets.MagicPixel.Value, fillRect, fill);

            // 边框
            DrawBorder(rect, border);

            // 文本
            string text = $"{label} {System.Math.Floor(value)}/{max}";
            var size = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(text);
            var pos = new Vector2(rect.Center.X - size.X / 2f, rect.Y - 16);
            Terraria.Utils.DrawBorderString(sb, text, pos, Color.White);
        }

        private static void DrawBorder(Rectangle rect, Color color)
        {
            var sb = Main.spriteBatch;
            var px = TextureAssets.MagicPixel.Value;
            // top
            sb.Draw(px, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
            // left
            sb.Draw(px, new Rectangle(rect.X, rect.Y, 2, rect.Height), color);
            // right
            sb.Draw(px, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), color);
            // bottom
            sb.Draw(px, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), color);
        }
    }
}
