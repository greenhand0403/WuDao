// BoidsFlocking.cs
// tModLoader 1.4+ 兼容的通用“鸟群/鱼群/粒子群”算法工具类
// 功能：分离(Separation)、对齐(Alignment)、聚合(Cohesion) + 轨迹跟随(Path Following)
// 可用于一群 Projectile、Gore、Dust 或自定义粒子对象的群体运动。
//
// 使用方式见文件底部的 ModProjectile 示例。
//
// 作者：Terraria Modding 专家（适配 tML，Vector2 来自 Microsoft.Xna.Framework）

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework; // Vector2

namespace WuDao.Common
{
    #region 数据结构
    public struct Boid
    {
        public int Id;               // 稳定排序使用（可用 whoAmI 或自定义）
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Accel;
        public float MaxSpeed;
        public float MaxForce;

        public Boid(int id, Vector2 pos, Vector2 initialVel, float maxSpeed, float maxForce)
        {
            Id = id;
            Position = pos;
            Velocity = initialVel;
            Accel = Vector2.Zero;
            MaxSpeed = MathF.Max(0.001f, maxSpeed);
            MaxForce = MathF.Max(0.0001f, maxForce);
        }
    }

    public class FlockParams
    {
        // 感知半径（像素）
        public float NeighborRadius = 180f;     // 感知邻居用于对齐和聚合
        public float SeparationRadius = 90f;    // 更小的半径用于分离

        // 权重
        public float SeparationWeight = 1.2f;
        public float AlignmentWeight = 1.0f;
        public float CohesionWeight = 0.9f;
        public float PathFollowWeight = 1.0f;
        public float NoiseWeight = 0.15f;       // 随机扰动（保持自然）

        // 路径跟随
        public float TrajectoryAdvancePerSecond = 420f; // 每秒前进的弧长速度（像素/秒）
        public float IndividualPhaseJitter = 120f;      // 每只个体相位偏移（像素），避免完全重叠

        // 速度/控制
        public float DesiredCruiseSpeed = 12f;  // 期望巡航速度（像素/帧，tML 每 tick ~1/60 秒）
        public float MaxSteerForce = 0.35f;     // 每 tick 最大转向力（像素/帧^2）

        // 其他
        public bool ClampInsidePathTube = false;  // 若为 true，会轻微把个体吸回路径附近
        public float PathTubeRadius = 160f;       // 路径“管道”半径

        public FlockParams() { }
    }

    #endregion

    #region 轨迹接口与 Catmull-Rom 样条实现
    public interface ITrajectory
    {
        // 输入参数 t ∈ [0, 1]，返回路径上的点与切线，且路径应近似弧长参数化
        Vector2 GetPoint(float t);
        Vector2 GetTangent(float t);
        float Length { get; }          // 近似总弧长（像素）
        float NormalizeArc(float s);   // 将弧长 s（像素）映射到 [0,1] 的参数 t
    }

    public class CatmullRomTrajectory : ITrajectory
    {
        private readonly List<Vector2> _points;
        private readonly bool _loop;
        private readonly float[] _accumLen; // 每段累积弧长
        private readonly float _totalLen;

        public float Length => _totalLen;

        public CatmullRomTrajectory(IReadOnlyList<Vector2> controlPoints, bool loop)
        {
            if (controlPoints == null || controlPoints.Count < 2)
                throw new ArgumentException("CatmullRomTrajectory 需要至少 2 个控制点。");

            _loop = loop;
            _points = new List<Vector2>(controlPoints.Count + (loop ? 0 : 2));

            // 为了端点更平滑：非 loop 情况下复制端点
            if (!loop) _points.Add(controlPoints[0]);
            _points.AddRange(controlPoints);
            if (!loop) _points.Add(controlPoints[controlPoints.Count - 1]);

            // 预计算弧长表（粗采样）
            const int samplesPerSeg = 16;
            int segCount = (_loop ? controlPoints.Count : controlPoints.Count - 1);
            _accumLen = new float[segCount + 1];
            float accum = 0f;
            Vector2 prev = GetPointRaw(0, 0f);
            for (int s = 0; s < segCount; s++)
            {
                for (int i = 1; i <= samplesPerSeg; i++)
                {
                    float u = i / (float)samplesPerSeg;
                    Vector2 p = GetPointRaw(s, u);
                    accum += (p - prev).Length();
                    prev = p;
                }
                _accumLen[s + 1] = accum;
            }
            _totalLen = MathF.Max(1f, accum);
        }

        // 段内原始 Catmull-Rom 求点
        private Vector2 GetPointRaw(int seg, float u)
        {
            int n = _points.Count;
            int p1 = WrapIndex(seg + 1, n);
            int p0 = WrapIndex(seg + 0, n);
            int p2 = WrapIndex(seg + 2, n);
            int p3 = WrapIndex(seg + 3, n);

            Vector2 P0 = _points[p0];
            Vector2 P1 = _points[p1];
            Vector2 P2 = _points[p2];
            Vector2 P3 = _points[p3];

            float u2 = u * u;
            float u3 = u2 * u;
            // Catmull-Rom 样条
            return 0.5f * ((2f * P1) + (-P0 + P2) * u + (2f * P0 - 5f * P1 + 4f * P2 - P3) * u2 + (-P0 + 3f * P1 - 3f * P2 + P3) * u3);
        }

        private Vector2 GetTangentRaw(int seg, float u)
        {
            int n = _points.Count;
            int p1 = WrapIndex(seg + 1, n);
            int p0 = WrapIndex(seg + 0, n);
            int p2 = WrapIndex(seg + 2, n);
            int p3 = WrapIndex(seg + 3, n);

            Vector2 P0 = _points[p0];
            Vector2 P1 = _points[p1];
            Vector2 P2 = _points[p2];
            Vector2 P3 = _points[p3];

            float u2 = u * u;
            // 一阶导
            return 0.5f * ((-P0 + P2) + 2f * (2f * P0 - 5f * P1 + 4f * P2 - P3) * u + 3f * (-P0 + 3f * P1 - 3f * P2 + P3) * u2);
        }

        private static int WrapIndex(int i, int n)
        {
            int m = i % n;
            return m < 0 ? m + n : m;
        }

        public Vector2 GetPoint(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            ToSeg(t, out int seg, out float u);
            return GetPointRaw(seg, u);
        }

        public Vector2 GetTangent(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            ToSeg(t, out int seg, out float u);
            Vector2 tan = GetTangentRaw(seg, u);
            if (tan.LengthSquared() < 0.0001f) return new Vector2(1f, 0f);
            return Vector2.Normalize(tan);
        }

        public float NormalizeArc(float s)
        {
            if (_totalLen <= 0f) return 0f;
            s = s % _totalLen; if (s < 0) s += _totalLen;
            // 在累积弧长表中二分查找
            int lo = 0, hi = _accumLen.Length - 1;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (_accumLen[mid] < s) lo = mid + 1; else hi = mid;
            }
            int seg = Math.Max(0, lo - 1);
            float segLen = _accumLen[seg + 1] - _accumLen[seg];
            float u = segLen > 0 ? (s - _accumLen[seg]) / segLen : 0f;
            // 把 (seg,u) 映射回 [0,1]
            float t = (seg + u) / (float)(_accumLen.Length - 1);
            return MathHelper.Clamp(t, 0f, 1f);
        }

        private void ToSeg(float t, out int seg, out float u)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float f = t * (_accumLen.Length - 1);
            seg = Math.Min(_accumLen.Length - 2, (int)MathF.Floor(f));
            u = f - seg;
        }
    }
    #endregion

    #region 群体更新
    public static class Boids
    {
        // 更新整群个体的动力学（原地修改 boids 列表）
        public static void Step(List<Boid> boids, ITrajectory trajectory, FlockParams p, float globalTimeSeconds, int worldSeed, float dt = 1f / 60f)
        {
            if (boids == null || boids.Count == 0) return;
            if (p == null) p = new FlockParams();

            // 便于 KD/网格优化，这里用 O(n^2) 简化，群规模 <= 200 时够用。如需更大，可换网格桶。
            int n = boids.Count;
            Span<Vector2> sepArr = stackalloc Vector2[n];
            Span<Vector2> aliArr = stackalloc Vector2[n];
            Span<Vector2> cohArr = stackalloc Vector2[n];
            Span<int> neiCnt = stackalloc int[n];
            Span<int> sepCnt = stackalloc int[n];

            float neighR2 = p.NeighborRadius * p.NeighborRadius;
            float sepR2 = p.SeparationRadius * p.SeparationRadius;

            // 预计算邻域统计
            for (int i = 0; i < n; i++)
            {
                Vector2 sumVel = Vector2.Zero;
                Vector2 sumPos = Vector2.Zero;
                Vector2 sumSep = Vector2.Zero;
                int neighbors = 0, seps = 0;
                Vector2 pi = boids[i].Position;
                for (int j = 0; j < n; j++) if (i != j)
                    {
                        Vector2 d = boids[j].Position - pi;
                        float d2 = d.LengthSquared();
                        if (d2 < neighR2)
                        {
                            neighbors++;
                            sumVel += boids[j].Velocity;
                            sumPos += boids[j].Position;
                            if (d2 < sepR2)
                            {
                                seps++;
                                float inv = 1f / MathF.Max(0.001f, MathF.Sqrt(d2));
                                sumSep -= d * inv; // 远离邻居
                            }
                        }
                    }
                neiCnt[i] = neighbors;
                sepCnt[i] = seps;
                aliArr[i] = sumVel;
                cohArr[i] = sumPos;
                sepArr[i] = sumSep;
            }

            // 计算 steering 并推进
            for (int i = 0; i < n; i++)
            {
                Boid b = boids[i];
                Vector2 accel = Vector2.Zero;

                // 分离
                if (sepCnt[i] > 0)
                {
                    Vector2 desired = Vector2.Normalize(sepArr[i]) * b.MaxSpeed - b.Velocity;
                    accel += Limit(desired, b.MaxForce) * p.SeparationWeight;
                }

                // 对齐
                if (neiCnt[i] > 0)
                {
                    Vector2 avgVel = aliArr[i] / neiCnt[i];
                    if (avgVel.LengthSquared() > 0.0001f)
                    {
                        Vector2 desired = Vector2.Normalize(avgVel) * b.MaxSpeed - b.Velocity;
                        accel += Limit(desired, b.MaxForce) * p.AlignmentWeight;
                    }
                }

                // 聚合
                if (neiCnt[i] > 0)
                {
                    Vector2 center = cohArr[i] / neiCnt[i];
                    Vector2 desired = Seek(b, center, b.MaxSpeed) - b.Velocity;
                    accel += Limit(desired, b.MaxForce) * p.CohesionWeight;
                }

                // 轨迹跟随（基于全局时间 + 个体相位）
                if (trajectory != null)
                {
                    float arc = p.TrajectoryAdvancePerSecond * globalTimeSeconds + HashPhase(worldSeed, b.Id, p.IndividualPhaseJitter);
                    float tOnPath = trajectory.NormalizeArc(arc);
                    Vector2 target = trajectory.GetPoint(tOnPath);
                    Vector2 tangent = trajectory.GetTangent(tOnPath);

                    // 目标速度：沿切线方向巡航
                    Vector2 desiredVel = Vector2.Normalize(tangent) * MathF.Max(2f, p.DesiredCruiseSpeed);
                    Vector2 steer = desiredVel - b.Velocity;
                    accel += Limit(steer, p.MaxSteerForce) * p.PathFollowWeight;

                    if (p.ClampInsidePathTube)
                    {
                        float dist = (b.Position - target).Length();
                        if (dist > p.PathTubeRadius)
                        {
                            // 轻微吸回路径
                            Vector2 back = Seek(b, target, b.MaxSpeed) - b.Velocity;
                            accel += Limit(back, b.MaxForce) * 0.5f;
                        }
                    }
                }

                // 轻微噪声（保持自然）
                if (p.NoiseWeight > 0f)
                {
                    Vector2 jitter = HashUnitVector(worldSeed, b.Id, globalTimeSeconds) * b.MaxForce;
                    accel += jitter * p.NoiseWeight;
                }

                // 积分推进
                b.Accel = accel;
                b.Velocity += b.Accel * dt;

                // 限速
                float vLen = b.Velocity.Length();
                float maxSpd = MathF.Max(2f, p.DesiredCruiseSpeed);
                if (vLen > maxSpd) b.Velocity *= maxSpd / vLen;

                b.Position += b.Velocity * dt * 60f; // 将 dt(秒) 转回“像素/帧”尺度，便于与原生逻辑对齐

                boids[i] = b;
            }
        }

        private static Vector2 Seek(in Boid b, Vector2 target, float speed)
        {
            Vector2 desired = target - b.Position;
            float len = desired.Length();
            if (len < 0.0001f) return b.Velocity;
            desired *= speed / len;
            return desired;
        }

        private static Vector2 Limit(Vector2 v, float max)
        {
            float l2 = v.LengthSquared();
            if (l2 > max * max)
            {
                float inv = max / MathF.Sqrt(l2);
                return v * inv;
            }
            return v;
        }

        private static float HashPhase(int seed, int id, float amplitude)
        {
            unchecked
            {
                int h = (int)2166136261;
                h = (h ^ seed) * 16777619;
                h = (h ^ id) * 16777619;
                // 0..1
                float u = ((h & 0x7FFFFFFF) / 2147483647f);
                return (u - 0.5f) * 2f * amplitude; // -A..A 像素偏移
            }
        }

        private static Vector2 HashUnitVector(int seed, int id, float t)
        {
            // 简易哈希 + 时间，使噪声方向缓慢变化
            float u = Frac(MathF.Sin((id * 127.1f + seed * 311.7f) + t * 0.7f) * 43758.5453f);
            float a = u * MathHelper.TwoPi;
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        }

        private static float Frac(float x) => x - MathF.Floor(x);
    }
    #endregion
}

// ============================ 使用示例（tML ModProjectile） ============================
// 把下面示例放入你的 Mod 内任意新建的 .cs 文件（同一命名空间），并根据需要修改。

/*
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MyMod.Common.AI; // 引入上面的命名空间

public class FlockManager
{
    // 简易管理：以 groupKey -> boids 列表的方式共享状态
    private static readonly Dictionary<int, List<Boid>> Groups = new();
    private static readonly Dictionary<int, ITrajectory> Trajs = new();
    private static readonly Dictionary<int, FlockParams> Params = new();

    public static void EnsureGroup(int groupKey, List<Vector2> waypoints, bool loop, FlockParams p)
    {
        if (!Groups.ContainsKey(groupKey))
        {
            Groups[groupKey] = new List<Boid>();
            Trajs[groupKey] = new CatmullRomTrajectory(waypoints, loop);
            Params[groupKey] = p ?? new FlockParams();
        }
    }

    public static void AddOrUpdateBoid(int groupKey, int id, Vector2 pos, Vector2 vel)
    {
        var list = Groups[groupKey];
        int idx = list.FindIndex(b => b.Id == id);
        if (idx < 0) list.Add(new Boid(id, pos, vel, maxSpeed: 18f, maxForce: 0.9f));
        else
        {
            var b = list[idx];
            b.Position = pos; // 同步当前弹体位置（初始帧）
            b.Velocity = vel;
            list[idx] = b;
        }
    }

    public static Vector2 StepAndGetVelocity(int groupKey, int id, float globalTime)
    {
        if (!Groups.TryGetValue(groupKey, out var list)) return Vector2.Zero;
        var traj = Trajs[groupKey];
        var p = Params[groupKey];
        Boids.Step(list, traj, p, globalTime, worldSeed: Main.instance != null ? Main.instance.GetHashCode() : 12345);
        // 取回该 id 的速度
        int idx = list.FindIndex(b => b.Id == id);
        if (idx >= 0) return list[idx].Velocity;
        return Vector2.Zero;
    }
}

public class BirdFlockProj : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        int owner = Projectile.owner;
        int groupKey = owner * 10000 + Projectile.type; // 每个玩家/弹种一群

        // 在首次使用时，配置路径与参数
        if (Projectile.localAI[0] == 0f)
        {
            Projectile.localAI[0] = 1f;
            var waypoints = new List<Vector2>
            {
                Main.player[owner].Center + new Vector2(0, -120),
                Main.player[owner].Center + new Vector2(200, -40),
                Main.player[owner].Center + new Vector2(0, 140),
                Main.player[owner].Center + new Vector2(-220, 0),
            };
            var p = new FlockParams
            {
                NeighborRadius = 180f,
                SeparationRadius = 70f,
                SeparationWeight = 1.25f,
                AlignmentWeight = 1.0f,
                CohesionWeight = 0.9f,
                PathFollowWeight = 1.15f,
                DesiredCruiseSpeed = 12f,
                MaxSteerForce = 0.45f,
                TrajectoryAdvancePerSecond = 380f,
                IndividualPhaseJitter = 160f,
                ClampInsidePathTube = true,
                PathTubeRadius = 220f,
            };
            FlockManager.EnsureGroup(groupKey, waypoints, loop: true, p);
        }

        // 同步/注册 boid
        FlockManager.AddOrUpdateBoid(groupKey, Projectile.whoAmI, Projectile.Center, Projectile.velocity);

        // 取回群算法给出的速度并应用
        float globalTime = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;
        Vector2 v = FlockManager.StepAndGetVelocity(groupKey, Projectile.whoAmI, globalTime);
        if (v != Vector2.Zero)
            Projectile.velocity = v;

        // 你也可以在此处生成 Dust/Gore 可视化尾迹
    }
}
*/