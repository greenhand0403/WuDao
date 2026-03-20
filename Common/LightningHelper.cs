using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Utilities;

namespace WuDao.Common
{
    // 模仿闪电珠弧的绘制，参考原版 main 里面的绘制方法
    public readonly record struct LightningPalette(
        Color OuterColor,
        Color MiddleColor,
        Color InnerColor,
        float OuterScale,
        float MiddleScale,
        float InnerScale,
        float OuterOpacity,
        float MiddleOpacity,
        float InnerOpacity
    );
    public static class LightningPalettes
    {
        /// <summary>
        /// 金色雷电：外层金橙、中层亮黄、内芯近白。
        /// 目标风格：善逸 / 雷之呼吸。
        /// </summary>
        public static readonly LightningPalette ZenitsuGold = new(
            OuterColor: new Color(255, 196, 64, 0),
            MiddleColor: new Color(255, 232, 128, 0),
            InnerColor: new Color(255, 255, 220, 0),
            OuterScale: 0.60f,
            MiddleScale: 0.40f,
            InnerScale: 0.20f,
            OuterOpacity: 0.55f,
            MiddleOpacity: 0.60f,
            InnerOpacity: 0.70f
        );
        public static readonly LightningPalette CultistArcWhip = new(
            OuterColor: new Color(115, 204, 219, 0),
            MiddleColor: new Color(113, 251, 255, 0),
            InnerColor: new Color(255, 255, 255, 0),
            OuterScale: 0.36f,
            MiddleScale: 0.24f,
            InnerScale: 0.12f,
            OuterOpacity: 0.50f,
            MiddleOpacity: 0.50f,
            InnerOpacity: 0.50f
        );
    }
    public static class LightningHelper
    {
        public static bool? CheckOldPosCollision(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < projectile.oldPos.Length; i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero)
                    break;

                Rectangle hitbox = projHitbox;
                hitbox.X = (int)projectile.oldPos[i].X;
                hitbox.Y = (int)projectile.oldPos[i].Y;

                if (hitbox.Intersects(targetHitbox))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 对任意一串世界坐标点做“连续线段碰撞”。
        /// 适合鞭子、电弧链、激光折线等。
        /// </summary>
        public static bool CheckPointChainCollision(IReadOnlyList<Vector2> points, Rectangle targetHitbox, float lineWidth)
        {
            if (points == null || points.Count < 2)
                return false;

            float collisionPoint = 0f;

            for (int i = 0; i < points.Count - 1; i++)
            {
                if (Collision.CheckAABBvLineCollision(
                    targetHitbox.TopLeft(),
                    targetHitbox.Size(),
                    points[i],
                    points[i + 1],
                    lineWidth,
                    ref collisionPoint))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 按 oldPos 绘制原版 466 风格闪电。
        /// </summary>
        public static void DrawLightningFromOldPos(Projectile projectile, SpriteBatch spriteBatch, LightningPalette palette)
        {
            Vector2 end = projectile.Center + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
            Texture2D lightningTexture = TextureAssets.Extra[ExtrasID.CultistLightingArc].Value;
            Vector2 projectileHalfSize = new(projectile.width * 0.5f, projectile.height * 0.5f);

            DrawLightningLayersFromOldPos(
                spriteBatch,
                lightningTexture,
                projectile.oldPos,
                projectileHalfSize,
                projectile.gfxOffY,
                end,
                projectile.scale,
                palette);
        }

        /// <summary>
        /// 按“已经是路径中心点”的点列绘制闪电。
        /// 适合鞭子的控制点、电刑轨迹点。
        /// </summary>
        public static void DrawLightningAlongPoints(
            SpriteBatch spriteBatch,
            IReadOnlyList<Vector2> worldPoints,
            float scale,
            LightningPalette palette)
        {
            if (worldPoints == null || worldPoints.Count < 2)
                return;

            Texture2D lightningTexture = TextureAssets.Extra[ExtrasID.CultistLightingArc].Value;

            DrawLightningLayer(spriteBatch, lightningTexture, worldPoints, scale * palette.OuterScale, palette.OuterColor * palette.OuterOpacity);
            DrawLightningLayer(spriteBatch, lightningTexture, worldPoints, scale * palette.MiddleScale, palette.MiddleColor * palette.MiddleOpacity);
            DrawLightningLayer(spriteBatch, lightningTexture, worldPoints, scale * palette.InnerScale, palette.InnerColor * palette.InnerOpacity);
        }

        private static void DrawLightningLayersFromOldPos(
            SpriteBatch spriteBatch,
            Texture2D lightningTexture,
            IReadOnlyList<Vector2> oldPos,
            Vector2 projectileHalfSize,
            float gfxOffY,
            Vector2 finalEnd,
            float scale,
            LightningPalette palette)
        {
            DrawOneOldPosLayer(spriteBatch, lightningTexture, oldPos, projectileHalfSize, gfxOffY, finalEnd,
                new Vector2(scale) * palette.OuterScale, palette.OuterColor * palette.OuterOpacity);

            DrawOneOldPosLayer(spriteBatch, lightningTexture, oldPos, projectileHalfSize, gfxOffY, finalEnd,
                new Vector2(scale) * palette.MiddleScale, palette.MiddleColor * palette.MiddleOpacity);

            DrawOneOldPosLayer(spriteBatch, lightningTexture, oldPos, projectileHalfSize, gfxOffY, finalEnd,
                new Vector2(scale) * palette.InnerScale, palette.InnerColor * palette.InnerOpacity);
        }

        private static void DrawOneOldPosLayer(
            SpriteBatch spriteBatch,
            Texture2D lightningTexture,
            IReadOnlyList<Vector2> oldPos,
            Vector2 projectileHalfSize,
            float gfxOffY,
            Vector2 finalEnd,
            Vector2 laserScale,
            Color drawColor)
        {
            DelegateMethods.c_1 = drawColor;
            DelegateMethods.f_1 = 1f;

            for (int i = oldPos.Count - 1; i > 0; i--)
            {
                if (oldPos[i] == Vector2.Zero)
                    continue;

                Vector2 start = oldPos[i] + projectileHalfSize + Vector2.UnitY * gfxOffY - Main.screenPosition;
                Vector2 end = oldPos[i - 1] + projectileHalfSize + Vector2.UnitY * gfxOffY - Main.screenPosition;

                Utils.DrawLaser(spriteBatch, lightningTexture, start, end, laserScale, DelegateMethods.LightningLaserDraw);
            }

            if (oldPos[0] != Vector2.Zero)
            {
                Vector2 start = oldPos[0] + projectileHalfSize + Vector2.UnitY * gfxOffY - Main.screenPosition;
                Utils.DrawLaser(spriteBatch, lightningTexture, start, finalEnd, laserScale, DelegateMethods.LightningLaserDraw);
            }
        }

        private static void DrawLightningLayer(
            SpriteBatch spriteBatch,
            Texture2D lightningTexture,
            IReadOnlyList<Vector2> worldPoints,
            float scale,
            Color drawColor)
        {
            DelegateMethods.c_1 = drawColor;
            DelegateMethods.f_1 = 1f;

            Vector2 laserScale = new(scale);

            for (int i = 0; i < worldPoints.Count - 1; i++)
            {
                Vector2 start = worldPoints[i] - Main.screenPosition;
                Vector2 end = worldPoints[i + 1] - Main.screenPosition;

                Utils.DrawLaser(spriteBatch, lightningTexture, start, end, laserScale, DelegateMethods.LightningLaserDraw);
            }
        }

        /// <summary>
        /// 根据一组“控制点”生成更像闪电的锯齿点列。
        /// 控制点通常来自 FillWhipControlPoints。
        /// </summary>
        public static void BuildJaggedLightningPath(
            IReadOnlyList<Vector2> controlPoints,
            List<Vector2> lightningPoints,
            int seed,
            int subdivisionsPerSegment = 2,
            float maxOffset = 10f)
        {
            lightningPoints.Clear();

            if (controlPoints == null || controlPoints.Count == 0)
                return;

            UnifiedRandom random = new(seed);

            lightningPoints.Add(controlPoints[0]);

            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                Vector2 start = controlPoints[i];
                Vector2 end = controlPoints[i + 1];
                Vector2 segment = end - start;

                if (segment.LengthSquared() <= 0.0001f)
                    continue;

                Vector2 segmentDirection = Vector2.Normalize(segment);
                Vector2 normal = new Vector2(-segmentDirection.Y, segmentDirection.X);

                for (int step = 1; step <= subdivisionsPerSegment; step++)
                {
                    float t = step / (float)(subdivisionsPerSegment + 1);
                    Vector2 point = Vector2.Lerp(start, end, t);

                    // 让闪电更像“中段更抖，首尾更稳”
                    float alongWholeWhip = (i + t) / Math.Max(1f, controlPoints.Count - 1f);
                    float envelope =
                        (float)Math.Sin(alongWholeWhip * Math.PI); // 两端小，中间大

                    float offset = (random.NextFloat() * 2f - 1f) * maxOffset * envelope;
                    point += normal * offset;

                    lightningPoints.Add(point);
                }

                lightningPoints.Add(end);
            }
        }
    }
}