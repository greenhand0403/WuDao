using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace WuDao.Content.DrawLayers
{
    // 已修改: 图标承担技能冷却指示器的功能，提示玩家绝学技能正在冷却中
    // 绝学系统：在人物之上再画一层绝学技能图标的虚影
    public class JuexueGhostLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;
            var qi = player.GetModPlayer<Players.QiPlayer>();
            if (qi.Ghost.TimeLeft <= 0)
                return;

            // —— 冷却图标透明度：剩余越多越不透明，剩余越少越透明 —— //
            int dur = qi.Ghost.Duration;
            float opacity = 0f;
            if (dur > 0)
                opacity = MathHelper.Clamp(qi.Ghost.TimeLeft / (float)dur, 0f, 1f);

            // 透明到 0 就不画（表示技能已就绪）
            if (opacity <= 0f)
                return;

            // —— 基础绘制参数 —— //
            Texture2D tex = qi.JueXueTex.Value;
            Rectangle src = qi.Ghost.Src.Width > 0 ? qi.Ghost.Src : tex.Bounds;
            Vector2 origin = src.Size() * 0.5f;
            Vector2 worldPos = player.Top + qi.Ghost.Offset; // 以玩家中心为基准
            Vector2 screenPos = worldPos - Main.screenPosition;

            // 可加一点轻微的漂浮/抖动效果（可选）
            float wobble = (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 6f) * 2f;
            screenPos.Y += wobble;
            // 玩家面对左边时将虚影沿水平方向翻转
            SpriteEffects spriteEffects = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            
            var color = Color.White * opacity;
            Main.EntitySpriteDraw(tex, screenPos, src, color, 0f, origin, qi.Ghost.Scale, spriteEffects, 0);
        }
    }
}
