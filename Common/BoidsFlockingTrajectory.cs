// ============================ 追加：四套“轨迹跟踪”实现 ============================
// ① 绕玩家圆周 ② 绕鼠标圆周 ③ 抛物线往返（玩家<->光标）④ 自动索敌：到怪物前方再回玩家
// 这些实现全部符合 ITrajectory 接口，并且是“动态轨迹”（中心/端点随游戏状态变化）。
// 直接在 FlockManager.EnsureGroup(...) 里用这些 Trajectory 替代 CatmullRomTrajectory 即可。

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Common
{
    // 工具：把 s(弧长) 映射为 0..1 的“往返三角波”参数
    internal static class Param
    {
        public static float PingPong01(float x)
        {
            float u = Frac(x);
            return u < 0.5f ? (u * 2f) : (2f - u * 2f);
        }
        public static float Frac(float x) => x - MathF.Floor(x);
    }

    // ① 绕着玩家一定半径做圆周运动
    public class PlayerCircleTrajectory : ITrajectory
    {
        private readonly int _owner;
        private readonly float _radius;
        public float Length => MathHelper.TwoPi * MathF.Max(4f, _radius);
        public PlayerCircleTrajectory(int owner, float radius)
        { _owner = owner; _radius = MathF.Max(4f, radius); }

        public Vector2 GetPoint(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float angle = t * MathHelper.TwoPi;
            Player plr = Main.player[_owner];
            Vector2 c = plr?.active == true ? plr.Center : Vector2.Zero;
            return c + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * _radius;
        }
        public Vector2 GetTangent(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float angle = t * MathHelper.TwoPi;
            // 圆的切线方向（逆时针）
            Vector2 tan = new(-MathF.Sin(angle), MathF.Cos(angle));
            return Vector2.Normalize(tan);
        }
        public float NormalizeArc(float s)
        {
            float L = Length; if (L <= 0f) return 0f;
            return (s % L + L) % L / L; // 单向绕圈
        }
    }

    // ② 绕着鼠标中心一定半径做圆周运动
    public class CursorCircleTrajectory : ITrajectory
    {
        private readonly float _radius;
        public float Length => MathHelper.TwoPi * MathF.Max(4f, _radius);
        public CursorCircleTrajectory(float radius) { _radius = MathF.Max(4f, radius); }

        public Vector2 GetPoint(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float angle = t * MathHelper.TwoPi;
            Vector2 c = Main.MouseWorld; // 世界坐标中的鼠标位置
            return c + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * _radius;
        }
        public Vector2 GetTangent(float t)
        {
            float angle = MathHelper.Clamp(t, 0f, 1f) * MathHelper.TwoPi;
            return Vector2.Normalize(new Vector2(-MathF.Sin(angle), MathF.Cos(angle)));
        }
        public float NormalizeArc(float s)
        {
            float L = Length; if (L <= 0f) return 0f;
            return (s % L + L) % L / L;
        }
    }

    // ③ 从玩家位置以“抛物线(Bezier)”飞到光标，再从光标飞回玩家，循环
    public class ParabolaPingPongTrajectory : ITrajectory
    {
        private readonly int _owner;
        private readonly float _arcHeight; // 抛物线抬升高度（像素）。可为负实现向下弧线。
        public ParabolaPingPongTrajectory(int owner, float arcHeight = 120f)
        { _owner = owner; _arcHeight = arcHeight; }

        // 粗略取总长=直线距离*1.3，实时变化
        public float Length
        {
            get
            {
                Vector2 A = Main.player[_owner]?.Center ?? Vector2.Zero;
                Vector2 B = Main.MouseWorld;
                float d = Vector2.Distance(A, B);
                return MathF.Max(32f, d * 1.3f) * 2f; // 往返两段
            }
        }

        public Vector2 GetPoint(float t)
        {
            // t∈[0,1]，利用三角波实现 A->B->A
            Vector2 A = Main.player[_owner]?.Center ?? Vector2.Zero;
            Vector2 B = Main.MouseWorld;
            float u = Param.PingPong01(t);
            // 二次 Bezier：A, C, B；C 为中点上抬
            Vector2 mid = (A + B) * 0.5f;
            Vector2 dir = Vector2.Normalize(new Vector2(-(B - A).Y, (B - A).X)); // 侧向法线
            Vector2 C = mid + dir * _arcHeight;
            return QuadBezier(A, C, B, u);
        }
        public Vector2 GetTangent(float t)
        {
            Vector2 A = Main.player[_owner]?.Center ?? Vector2.Zero;
            Vector2 B = Main.MouseWorld;
            float u = Param.PingPong01(t);
            Vector2 mid = (A + B) * 0.5f;
            Vector2 dir = Vector2.Normalize(new Vector2(-(B - A).Y, (B - A).X));
            Vector2 C = mid + dir * _arcHeight;
            return Vector2.Normalize(QuadBezierTangent(A, C, B, u));
        }
        public float NormalizeArc(float s)
        {
            float L = Length; if (L <= 0f) return 0f;
            float cyc = (s % L + L) % L / L; // 0..1
            return cyc;
        }

        private static Vector2 QuadBezier(Vector2 A, Vector2 C, Vector2 B, float u)
        {
            float v = 1f - u;
            return v * v * A + 2f * v * u * C + u * u * B;
        }
        private static Vector2 QuadBezierTangent(Vector2 A, Vector2 C, Vector2 B, float u)
        {
            // 导数：2(1-u)(C-A) + 2u(B-C)
            return 2f * (1f - u) * (C - A) + 2f * u * (B - C);
        }
    }

    // ④ 自动索敌：飞到敌怪前方(预判点或朝向前) -> 再飞回玩家，循环
    public class EnemyFrontPingPongTrajectory : ITrajectory
    {
        private readonly int _owner;
        private readonly float _leadDist;     // 到敌人“前方”的距离
        private readonly float _searchRange;  // 索敌范围
        private readonly int _targetMask;     // 可扩展：用于筛选 NPC（此处简单保留）

        public EnemyFrontPingPongTrajectory(int owner, float leadDist = 120f, float searchRange = 900f, int targetMask = -1)
        { _owner = owner; _leadDist = leadDist; _searchRange = searchRange; _targetMask = targetMask; }

        public float Length
        {
            get
            {
                (Vector2 A, Vector2 P) = GetEndpoints();
                float d = Vector2.Distance(A, P);
                return MathF.Max(64f, d * 1.2f) * 2f;
            }
        }

        public Vector2 GetPoint(float t)
        {
            var (A, P) = GetEndpoints(); // A=玩家, P=目标前方
            float u = Param.PingPong01(t);
            // 轻微的弧线（比直线更自然）
            Vector2 mid = (A + P) * 0.5f;
            Vector2 dir = Vector2.Normalize(new Vector2(-(P - A).Y, (P - A).X));
            Vector2 C = mid + dir * 80f; // 固定抬升
            return QuadBezier(A, C, P, u);
        }
        public Vector2 GetTangent(float t)
        {
            var (A, P) = GetEndpoints();
            float u = Param.PingPong01(t);
            Vector2 mid = (A + P) * 0.5f;
            Vector2 dir = Vector2.Normalize(new Vector2(-(P - A).Y, (P - A).X));
            Vector2 C = mid + dir * 80f;
            return Vector2.Normalize(QuadBezierTangent(A, C, P, u));
        }
        public float NormalizeArc(float s)
        {
            float L = Length; if (L <= 0f) return 0f;
            return (s % L + L) % L / L;
        }

        private (Vector2 player, Vector2 enemyFront) GetEndpoints()
        {
            Player plr = Main.player[_owner];
            Vector2 A = plr?.Center ?? Vector2.Zero;
            NPC target = FindTarget(A);
            if (target == null)
            {
                // 没有目标：退化为绕玩家小圆
                Vector2 Pdef = A + new Vector2(_leadDist, 0f);
                return (A, Pdef);
            }
            // 计算“前方”点：基于 NPC 速度。如果速度过小，用朝向或从玩家到 NPC 的方向。
            Vector2 facing;
            if (target.velocity.LengthSquared() > 0.25f)
                facing = Vector2.Normalize(target.velocity);
            else if (target.direction != 0)
                facing = new Vector2(target.direction, 0f);
            else
                facing = Vector2.Normalize(target.Center - A);

            Vector2 P = target.Center + facing * _leadDist;
            return (A, P);
        }

        private NPC FindTarget(Vector2 from)
        {
            NPC best = null; float bestD2 = _searchRange * _searchRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n == null || !n.active || n.friendly || n.dontTakeDamage) continue;
                if (!n.CanBeChasedBy()) continue;
                float d2 = Vector2.DistanceSquared(from, n.Center);
                if (d2 < bestD2)
                {
                    best = n; bestD2 = d2;
                }
            }
            return best;
        }

        private static Vector2 QuadBezier(Vector2 A, Vector2 C, Vector2 B, float u)
        {
            float v = 1f - u;
            return v * v * A + 2f * v * u * C + u * u * B;
        }
        private static Vector2 QuadBezierTangent(Vector2 A, Vector2 C, Vector2 B, float u)
        {
            return 2f * (1f - u) * (C - A) + 2f * u * (B - C);
        }
    }
}

// ============================ 使用示例（四种模式） ============================
/*
// 在你的 ModProjectile.AI() 首帧里：
int owner = Projectile.owner;
int groupKey = owner * 10000 + Projectile.type;

// ① 绕玩家圆周：
FlockManager.EnsureGroup(groupKey,
    waypoints: null, // 该参数在此实现中无效，可传 null
    loop: true,
    p: new FlockParams{ TrajectoryAdvancePerSecond = 420f, DesiredCruiseSpeed = 12f });
FlockManager.Trajs[groupKey] = new PlayerCircleTrajectory(owner, radius: 220f);

// ② 绕鼠标圆周：
FlockManager.Trajs[groupKey] = new CursorCircleTrajectory(radius: 200f);

// ③ 抛物线往返（玩家<->光标）：
FlockManager.Trajs[groupKey] = new ParabolaPingPongTrajectory(owner, arcHeight: 140f);

// ④ 自动索敌：到目标前方 <-> 回玩家：
FlockManager.Trajs[groupKey] = new EnemyFrontPingPongTrajectory(owner, leadDist: 160f, searchRange: 1000f);

// 其余逻辑同前：每帧 AddOrUpdateBoid(...)，然后 StepAndGetVelocity(...)
*/
