using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Audio;
using System;

namespace WuDao.Content.Projectiles.Ranged
{
    /// <summary>
    /// 圣光裁决射弹效果，瞬发四边形（顶角30°、底角90°）命中 + 白金尘视觉
    /// 轴心 = 底部顶点（命中点）
    /// ai[0] = 朝向角（弧度）
    /// localAI[0] = b（横向半宽） —— a 将按 a = b*(1+cot15°)/2 自动计算
    /// </summary>
    public class HolyKiteBurstHitbox : ModProjectile
    {
        private const int EdgeSamples = 18;
        private const int DustA = DustID.GemDiamond;
        private const int DustB = DustID.FireworkFountain_Yellow;
        public override string Texture => "WuDao/Content/Projectiles/Ranged/BrightVerdictProjectile";
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.timeLeft = 1;                 // 瞬发1帧
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            Projectile.DamageType = DamageClass.Ranged;

            // 让4片共享冷却，不会对同一只怪连打4次
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            float angle = Projectile.ai[0];
            Vector2 v = new Vector2(0f, -1f).RotatedBy(angle); // 向上
            Vector2 u = v.RotatedBy(MathHelper.PiOver2);       // 向右

            float b = Math.Abs(Projectile.localAI[0]);         // 横向半宽
            float cot15 = 3.7320508f;                          // cot(15°)
            float a = b * (1f + cot15) * 0.5f;                 // 纵向半轴
            float yMid = b;                                    // 侧点的纵向位置（自底尖起）

            Vector2 pivot = Projectile.Center;                 // 底尖
            Vector2 P_bottom = pivot;
            Vector2 P_top = pivot + v * (2f * a);
            Vector2 P_left = pivot + v * yMid - u * b;
            Vector2 P_right = pivot + v * yMid + u * b;

            // 白金尘：沿四条边撒
            SpawnEdgeDust(P_bottom, P_right);
            SpawnEdgeDust(P_right, P_top);
            SpawnEdgeDust(P_top, P_left);
            SpawnEdgeDust(P_left, P_bottom);

            Lighting.AddLight((P_top + P_bottom) * 0.5f, 0.95f, 0.95f, 0.9f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float angle = Projectile.ai[0];
            Vector2 v = new Vector2(0f, -1f).RotatedBy(angle);
            Vector2 u = v.RotatedBy(MathHelper.PiOver2);

            float b = Math.Abs(Projectile.localAI[0]);
            float cot15 = 3.7320508f;
            float a = b * (1f + cot15) * 0.5f;
            float yMid = b;

            Vector2 pivot = Projectile.Center;
            Vector2 P_bottom = pivot;
            Vector2 P_top = pivot + v * (2f * a);
            Vector2 P_left = pivot + v * yMid - u * b;
            Vector2 P_right = pivot + v * yMid + u * b;

            // 1) 快速：点在多边形内（测试 AABB 中心 + 四角）
            if (PointInKite(targetHitbox.Center.ToVector2(), pivot, u, v, a, b)) return true;

            Vector2 tl = new Vector2((float)targetHitbox.Left, (float)targetHitbox.Top);
            Vector2 tr = new Vector2((float)targetHitbox.Right, (float)targetHitbox.Top);
            Vector2 bl = new Vector2((float)targetHitbox.Left, (float)targetHitbox.Bottom);
            Vector2 br = new Vector2((float)targetHitbox.Right, (float)targetHitbox.Bottom);
            if (PointInKite(tl, pivot, u, v, a, b)) return true;
            if (PointInKite(tr, pivot, u, v, a, b)) return true;
            if (PointInKite(bl, pivot, u, v, a, b)) return true;
            if (PointInKite(br, pivot, u, v, a, b)) return true;

            // 2) 保险：AABB 与四条边是否相交（线段-矩形）
            if (Terraria.Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                P_bottom, P_right)) return true;
            if (Terraria.Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                P_right, P_top)) return true;
            if (Terraria.Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                P_top, P_left)) return true;
            if (Terraria.Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                P_left, P_bottom)) return true;

            return false;
        }

        // 解析式“点在四边形内”（局部坐标从底尖开始）
        private static bool PointInKite(Vector2 p, Vector2 pivot, Vector2 u, Vector2 v, float a, float b)
        {
            // p' = (x', y')，其中 y' 为自底尖向上的距离；x' 为水平位移
            Vector2 d = p - pivot;
            float xp = Vector2.Dot(d, u);
            float yp = Vector2.Dot(d, v);
            if (yp < 0f || yp > 2f * a) return false;

            // 0..b：从底尖到左右侧点 —— |x| <= y   （使底角=90°）
            // b..2a：从左右侧点到顶尖 —— |x| <= tan(15°) * (2a - y)  （使顶角=30°）
            float bound = (yp <= b) ? yp : (0.26794919f * (2f * a - yp)); // tan(15°)=~0.26794919
            return Math.Abs(xp) <= Math.Max(0f, bound);
        }

        private void SpawnEdgeDust(Vector2 p0, Vector2 p1)
        {
            for (int i = 0; i <= EdgeSamples; i++)
            {
                float t = i / (float)EdgeSamples;
                Vector2 pos = Vector2.Lerp(p0, p1, t);

                int d = Dust.NewDust(pos, 0, 0, DustA, 0, 0, 0, default, 1.0f + Main.rand.NextFloat(0.35f));
                var a = Main.dust[d];
                a.noGravity = true;
                a.noLight = false;
                a.velocity = (pos - Projectile.Center).SafeNormalize(Vector2.UnitY) * (4.8f * (0.9f + Main.rand.NextFloat(0.3f)))
                             + Main.rand.NextVector2Circular(0.5f, 0.5f);

                if (Main.rand.NextBool(3))
                {
                    int d2 = Dust.NewDust(pos, 0, 0, DustB, 0, 0, 0, default, 0.9f + Main.rand.NextFloat(0.25f));
                    var b = Main.dust[d2];
                    b.noGravity = true;
                    b.noLight = false;
                    b.velocity = (pos - Projectile.Center).SafeNormalize(Vector2.UnitY) * 3.8f
                                 + Main.rand.NextVector2Circular(0.4f, 0.4f);
                }
            }
        }
    }
}
