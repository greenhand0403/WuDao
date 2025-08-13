using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Audio;
using System;

namespace WuDao.Content.Projectiles.Ranged
{
    /// <summary>
    /// 旋转“菱形（四尖）”命中判定 + 撒尘。
    /// 轴心 = 菱形的“底部顶点”（始终不动），围绕它旋转。
    ///
    /// 传参：
    /// ai[0] = 初始角度（弧度）；ai[1] = 旋转总圈数（float，可为 4f）
    /// ai[2] = 旋转总时长（帧），例如 20
    /// localAI[0] = a（沿“上/下”方向的半轴长度，底尖到中心的距离）
    /// localAI[1] = b（沿“左/右”方向的半轴长度）
    ///
    /// 注意：本弹体的 Center 即为“底尖轴心”的坐标（不是中心点）。
    /// </summary>
    public class HolyDiamondSpinHitbox : ModProjectile
    {
        // —— 可调：视觉与密度 ——
        private const int EdgePoints = 18;     // 每条边的取样点数（越大越密）
        private const float DustSpeed = 4.8f;  // 撒尘外扩速度
        private const int DustPerPoint = 1;    // 每个点多少粒子
        private const int LightDustID = DustID.GemDiamond;              // 主色：圣光白
        private const int WarmDustID = DustID.FireworkFountain_Yellow; // 附色：暖金
        public override string Texture => "WuDao/Content/Projectiles/Ranged/BrightVerdictProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.timeLeft = 2;     // 先给个默认，实际会在 Spawn 时改
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;   // 可命中多体
            Projectile.hide = true;      // 不渲染贴图
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.DamageType = DamageClass.Ranged;
        }

        private float TotalRot => Projectile.ai[1];     // 旋转圈数
        private float TotalTime => Projectile.ai[2];    // 总时长（帧）
        private float A => Projectile.localAI[0];       // 纵向半轴
        private float B => Projectile.localAI[1];       // 横向半轴

        public override void AI()
        {
            // 以“底尖”为轴心旋转：菱形中心 = 轴心 + v * A
            float t = 1f - (Projectile.timeLeft / Math.Max(1f, TotalTime));
            float angle = Projectile.ai[0] + MathHelper.TwoPi * TotalRot * t; // 线性转角
            Vector2 v = new Vector2(0f, -1f).RotatedBy(angle);      // “竖直”基向量（向上）
            Vector2 u = v.RotatedBy(MathHelper.PiOver2);            // “水平”基向量（向右）

            Vector2 pivot = Projectile.Center;                      // 轴心=底尖
            Vector2 center = pivot + v * A;                         // 菱形中心

            // —— 视觉：沿四条边撒“白+金”尘，四尖清晰 ——
            SpawnDiamondEdgeDust(center, u, v, A, B, pivot, angle);

            // —— 命中：对目标 AABB 做“旋转菱形(L1)”包含测试（角度实时变化） ——
            // 交由 Colliding 实现；这里不需要做别的

            // 结束
            if (Projectile.timeLeft <= 1)
                Projectile.Kill();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 旋转菱形（L1球）： |dot(d,u)|/B + |dot(d,v)|/A <= 1  其中 d = 点 - center
            // 轴心为底尖 pivot，中心 center = pivot + v*A
            float t = 1f - (Projectile.timeLeft / Math.Max(1f, TotalTime));
            float angle = Projectile.ai[0] + MathHelper.TwoPi * TotalRot * t;
            Vector2 v = new Vector2(0f, -1f).RotatedBy(angle);
            Vector2 u = v.RotatedBy(MathHelper.PiOver2);
            Vector2 pivot = Projectile.Center;
            Vector2 center = pivot + v * A;

            // 测试：目标 AABB 的四角 + 中心
            Vector2 c = targetHitbox.Center.ToVector2();
            if (PointInDiamond(c, center, u, v, A, B)) return true;
            Vector2 tl = new Vector2(targetHitbox.Left, targetHitbox.Top);
            Vector2 tr = new Vector2(targetHitbox.Right, targetHitbox.Top);
            Vector2 bl = new Vector2(targetHitbox.Left, targetHitbox.Bottom);
            Vector2 br = new Vector2(targetHitbox.Right, targetHitbox.Bottom);
            if (PointInDiamond(tl, center, u, v, A, B)) return true;
            if (PointInDiamond(tr, center, u, v, A, B)) return true;
            if (PointInDiamond(bl, center, u, v, A, B)) return true;
            if (PointInDiamond(br, center, u, v, A, B)) return true;

            // 可选：补充一条从 AABB 中心到菱形中心的线段与边是否相交的快速近似（通常不需要）
            return false;
        }

        private static bool PointInDiamond(Vector2 p, Vector2 center, Vector2 u, Vector2 v, float a, float b)
        {
            Vector2 d = p - center;
            float cu = Math.Abs(Vector2.Dot(d, u)) / Math.Max(1e-3f, b);
            float cv = Math.Abs(Vector2.Dot(d, v)) / Math.Max(1e-3f, a);
            return cu + cv <= 1f;
        }

        private void SpawnDiamondEdgeDust(Vector2 center, Vector2 u, Vector2 v, float a, float b, Vector2 pivot, float angle)
        {
            // 顶点（按底→右→上→左顺序，能看出“底尖固定在轴心”）
            Vector2 P_bottom = pivot;          // 底尖固定
            Vector2 P_right = center + u * b; // 右尖
            Vector2 P_top = center + v * a; // 顶尖（= center + v*a = pivot + 2a*v）
            Vector2 P_left = center - u * b; // 左尖

            // 四条边线性取样
            SpawnEdge(P_bottom, P_right, u, v);
            SpawnEdge(P_right, P_top, u, v);
            SpawnEdge(P_top, P_left, u, v);
            SpawnEdge(P_left, P_bottom, u, v);

            // 一点发光
            Lighting.AddLight(center, 0.9f, 0.9f, 0.8f);
        }

        private void SpawnEdge(Vector2 p0, Vector2 p1, Vector2 u, Vector2 v)
        {
            for (int i = 0; i <= EdgePoints; i++)
            {
                float t = i / (float)EdgePoints;
                Vector2 pos = Vector2.Lerp(p0, p1, t);

                // 朝外法线（从菱形中心方向）：用 pos→顶点方向的插值来近似外法线，够好看、够快
                Vector2 outward = (pos - Projectile.Center);
                if (outward.LengthSquared() < 1e-4f) outward = v; // 极近轴心的点，给个稳定方向
                outward.Normalize();

                for (int k = 0; k < DustPerPoint; k++)
                {
                    int d = Dust.NewDust(pos, 0, 0, LightDustID, 0, 0, 0, default, 1.0f + Main.rand.NextFloat(0.35f));
                    var dd = Main.dust[d];
                    dd.noGravity = true;
                    dd.noLight = false;
                    dd.velocity = outward * (DustSpeed * (0.9f + Main.rand.NextFloat(0.3f)))
                                  + Main.rand.NextVector2Circular(0.5f, 0.5f);

                    if (Main.rand.NextBool(3))
                    {
                        int d2 = Dust.NewDust(pos, 0, 0, WarmDustID, 0, 0, 0, default, 0.9f + Main.rand.NextFloat(0.25f));
                        var dd2 = Main.dust[d2];
                        dd2.noGravity = true;
                        dd2.noLight = false;
                        dd2.velocity = outward * (DustSpeed * 0.8f) + Main.rand.NextVector2Circular(0.4f, 0.4f);
                    }
                }
            }
        }
    }
}
