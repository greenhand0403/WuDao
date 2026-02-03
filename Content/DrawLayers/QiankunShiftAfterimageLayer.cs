using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.DrawLayers
{
    //
    // 乾坤大挪移：玩家残影绘制
    public class QiankunShiftAfterimageLayer : PlayerDrawLayer
    {
        // 放在玩家主体之后，这样残影在后、玩家在前（更自然）
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player p = drawInfo.drawPlayer;
            var qi = p.GetModPlayer<QiPlayer>();
            if (!qi.ShiftActive || qi.ShiftTrailCount <= 1)
                return;

            // 基线：当前缓存里已经写好的所有“玩家本体”的 DrawData 数量
            int baseCount = drawInfo.DrawDataCache.Count;
            if (baseCount == 0)
                return;

            Vector2 currentCenter = p.Center;

            // 轨迹从旧到新：越旧越淡、越小的偏色
            for (int i = 0; i < qi.ShiftTrailCount; i++)
            {
                float k = i / (float)qi.ShiftTrailCount;      // 0..1
                float alpha = MathHelper.Lerp(0.6f, 1.0f, k); // 旧 -> 新，透明度从低到高
                Color tint = new Color(180, 220, 255) * alpha;

                Vector2 trailPos = qi.GetShiftTrailPos(i);
                Vector2 delta = trailPos - currentCenter;     // 残影相对位移

                // 只克隆“本体”的那一批切片，避免把我们自己 Add 的残影再次克隆
                for (int j = 0; j < baseCount; j++)
                {
                    DrawData dd = drawInfo.DrawDataCache[j];

                    // 平移到残影位置（DrawData.position 是屏幕坐标，delta 需要减去屏幕滚动差）
                    DrawData clone = dd;
                    clone.position += delta;           // 因为 dd 已是屏幕坐标，delta 也应是世界坐标差。二者一致，无需再减 screenPosition
                    // clone.color = MultiplyColor(dd.color, tint);
                    // 可选：稍微缩小越旧的残影（想要同尺寸可注释掉）
                    // clone.scale *= MathHelper.Lerp(0.9f, 1f, k);

                    drawInfo.DrawDataCache.Add(clone);
                }
            }
        }

        // 颜色相乘（保留原有色相明度的乘区）
        private static Color MultiplyColor(Color a, Color b)
        {
            // 按分量相乘，并把 alpha 取乘积
            return new Color(
                (byte)(a.R * b.R / 255),
                (byte)(a.G * b.G / 255),
                (byte)(a.B * b.B / 255),
                (byte)(a.A * b.A / 255)
            );
        }
    }
    //
}
