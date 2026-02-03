using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace WuDao.Content.DrawLayers
{
    // TODO: 图标应当承担技能冷却指示器的功能，提示玩家绝学技能正在冷却中
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

            // —— 计算淡入淡出透明度 —— //
            int dur = qi.Ghost.Duration;
            int age = dur - qi.Ghost.TimeLeft; // 已经过的tick
            const int fadeTicks = 12;          // 0.2s淡入/淡出（60FPS）
            float fadeIn = Utils.GetLerpValue(0, fadeTicks, age, clamped: true);
            float fadeOut = Utils.GetLerpValue(dur, dur - fadeTicks, age, clamped: true);
            float opacity = fadeIn * fadeOut; // 先入后出
            opacity *= 0.9f;                  // 稍微淡一点更像“虚影”

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
