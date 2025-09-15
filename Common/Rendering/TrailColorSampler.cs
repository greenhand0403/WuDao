using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace WuDao.Common.Rendering
{
    public enum DiagDir
    {
        // 从右上 → 左下（你之前的需求）
        RightUp_to_LeftDown,
        // 从左下 → 右上（新增）
        LeftDown_to_RightUp
    }

    public enum ExcludeMode
    {
        None,               // 不剔除
        ExcludePureBlack,   // 剔除纯黑
        ExcludeTransparent, // 剔除完全透明
        ExcludeBlackOrTransparent // 纯黑 或 完全透明 都剔除
    }

    public static partial class TrailColorSampler
    {
        // 简易缓存：按贴图引用 + 参数缓存结果，避免每帧 GetData
        private struct Key : IEquatable<Key>
        {
            public int TexHash;
            public int Samples;
            public DiagDir Dir;
            public ExcludeMode Ex;
            public int BandW;
            public bool Equals(Key o) => TexHash == o.TexHash && Samples == o.Samples && Dir == o.Dir && Ex == o.Ex && BandW == o.BandW;
            public override int GetHashCode() => HashCode.Combine(TexHash, Samples, (int)Dir, (int)Ex, BandW);
        }
        private static readonly Dictionary<Key, Color[]> _cache = new();

        /// <summary>
        /// 沿 45° 对角线采样（可选方向），按“带宽”做邻域平均，并按剔除规则过滤像素；返回 N 段平均色。
        /// </summary>
        public static Color[] SampleDiagonalColors(Texture2D tex, int samples, DiagDir dir,
                                                   ExcludeMode exclude, int bandWidth = 1,
                                                   byte blackThreshold = 6)
        {
            if (tex == null || tex.IsDisposed) return Array.Empty<Color>();
            int W = tex.Width, H = tex.Height;
            int N = Math.Max(2, Math.Min(samples, Math.Min(W, H)));

            var key = new Key
            {
                TexHash = RuntimeHelpers.GetHashCode(tex),
                Samples = N,
                Dir = dir,
                Ex = exclude,
                BandW = bandWidth
            };
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var pixels = new Color[W * H];
            tex.GetData(pixels);

            // 剔除规则
            bool ExBlack(Color c) => c.R <= blackThreshold && c.G <= blackThreshold && c.B <= blackThreshold;
            bool ExTrans(Color c) => c.A == 0;
            bool ShouldSkip(Color c) => exclude switch
            {
                ExcludeMode.None => false,
                ExcludeMode.ExcludePureBlack => ExBlack(c),
                ExcludeMode.ExcludeTransparent => ExTrans(c),
                ExcludeMode.ExcludeBlackOrTransparent => ExBlack(c) || ExTrans(c),
                _ => false
            };

            var outCols = new Color[N];
            for (int i = 0; i < N; i++)
            {
                // t: 0..1 沿对角线
                float t = i / (float)(N - 1);

                int x0, y0;
                if (dir == DiagDir.RightUp_to_LeftDown)
                {
                    // 右上 (W-1,0) → 左下 (0,H-1)
                    x0 = (int)Math.Round((W - 1) * (1f - t));
                    y0 = (int)Math.Round((H - 1) * (t));
                }
                else
                {
                    // 左下 (0,H-1) → 右上 (W-1,0)
                    x0 = (int)Math.Round((W - 1) * (t));
                    y0 = (int)Math.Round((H - 1) * (1f - t));
                }

                long r = 0, g = 0, b = 0, a = 0; int cnt = 0;
                for (int dy = -bandWidth; dy <= bandWidth; dy++)
                {
                    for (int dx = -bandWidth; dx <= bandWidth; dx++)
                    {
                        int x = x0 + dx, y = y0 + dy;
                        if ((uint)x >= (uint)W || (uint)y >= (uint)H) continue;
                        var c = pixels[y * W + x];
                        if (ShouldSkip(c)) continue;
                        r += c.R; g += c.G; b += c.B; a += c.A; cnt++;
                    }
                }

                if (cnt == 0)
                {
                    // 如果被剔除全挡住了，退化用原像素（不剔除）再取一次，
                    // 或者给一个默认值，这里给透明白：
                    outCols[i] = new Color(255, 255, 255, 0);
                }
                else
                {
                    outCols[i] = new Color((int)(r / cnt), (int)(g / cnt), (int)(b / cnt), (int)(a / cnt));
                }
            }

            _cache[key] = outCols;
            return outCols;
        }
    }

    public static partial class TrailColorSampler
    {
        public enum WeightProfile { Gaussian, Triangle }

        private struct RowKey : IEquatable<RowKey>
        {
            public int TexHash, Samples, RowRadius;
            public DiagDir Dir;
            public ExcludeMode Ex;
            public WeightProfile Profile;
            public float Sigma;
            public byte BlackThr;
            public bool Equals(RowKey o) =>
                TexHash == o.TexHash && Samples == o.Samples && RowRadius == o.RowRadius &&
                Dir == o.Dir && Ex == o.Ex && Profile == o.Profile &&
                Math.Abs(Sigma - o.Sigma) < 1e-6f && BlackThr == o.BlackThr;
            public override int GetHashCode() =>
                HashCode.Combine(TexHash, Samples, RowRadius, (int)Dir, (int)Ex, (int)Profile, Sigma, BlackThr);
        }

        private static readonly Dictionary<RowKey, Color[]> _cacheRowWeighted = new();

        /// <summary>
        /// 沿 45° 对角线的“每行加权平均”：对每个采样点 i，定位到该行 (y0)，
        /// 以对角线在该行的交点 xCenter 为中心，对这一行像素按 |x - xCenter| 做权重（越近权重越大）；
        /// 可选地向上下扩展 rowRadius 行再做行间平均（一般设 0~2）。
        /// </summary>
        /// <param name="tex">源贴图（武器贴图或刀光贴图）</param>
        /// <param name="samples">输出颜色段数（通常 = trailLen）</param>
        /// <param name="dir">方向：右上→左下 / 左下→右上</param>
        /// <param name="exclude">过滤模式：不剔除/剔黑/剔透明/剔黑或透明</param>
        /// <param name="rowRadius">可选：行邻域半径（0=只取一行；1=上下各一行参与）</param>
        /// <param name="sigma">横向权重带宽（像素）。Gaussian 下为标准差；Triangle 下为半径。</param>
        /// <param name="profile">权重曲线：Gaussian（平滑）或 Triangle（线性下降）</param>
        /// <param name="blackThreshold">纯黑判定阈值</param>
        public static Color[] SampleDiagonalRowWeightedColors(
            Texture2D tex, int samples, DiagDir dir,
            ExcludeMode exclude, int rowRadius = 0,
            float sigma = 6f, WeightProfile profile = WeightProfile.Gaussian,
            byte blackThreshold = 6)
        {
            if (tex == null || tex.IsDisposed) return Array.Empty<Color>();
            int W = tex.Width, H = tex.Height;
            int N = Math.Max(2, Math.Min(samples, Math.Min(W, H)));

            var key = new RowKey
            {
                TexHash = RuntimeHelpers.GetHashCode(tex),
                Samples = N,
                RowRadius = Math.Max(0, rowRadius),
                Dir = dir,
                Ex = exclude,
                Profile = profile,
                Sigma = Math.Max(0.001f, sigma),
                BlackThr = blackThreshold
            };
            if (_cacheRowWeighted.TryGetValue(key, out var cached))
                return cached;

            var pixels = new Color[W * H];
            tex.GetData(pixels);

            bool ExBlack(Color c) => c.R <= blackThreshold && c.G <= blackThreshold && c.B <= blackThreshold;
            bool ExTrans(Color c) => c.A == 0;
            bool Skip(Color c) => exclude switch
            {
                ExcludeMode.None => false,
                ExcludeMode.ExcludePureBlack => ExBlack(c),
                ExcludeMode.ExcludeTransparent => ExTrans(c),
                ExcludeMode.ExcludeBlackOrTransparent => ExBlack(c) || ExTrans(c),
                _ => false
            };

            // 横向权重函数
            float WtX(int dxAbs)
            {
                if (profile == WeightProfile.Triangle)
                {
                    float t = 1f - dxAbs / Math.Max(1f, sigma);
                    return t > 0 ? t : 0f;
                }
                // Gaussian
                float d = dxAbs / Math.Max(0.001f, sigma);
                return (float)Math.Exp(-0.5f * d * d);
            }

            var outCols = new Color[N];

            for (int i = 0; i < N; i++)
            {
                float t = i / (float)(N - 1);

                // 计算该采样点对应的对角线位置 (xCenter, yCenter)
                float xCenter, yCenter;
                if (dir == DiagDir.RightUp_to_LeftDown)
                {
                    xCenter = (W - 1) * (1f - t);
                    yCenter = (H - 1) * (t);
                }
                else
                {
                    xCenter = (W - 1) * (t);
                    yCenter = (H - 1) * (1f - t);
                }

                long r = 0, g = 0, b = 0, a = 0; double wSum = 0;

                // 行采样
                // int y0 = (int)Math.Round(yCenter);

                // 行邻域：只要 rowRadius > 0，就把上下行加进来（可给行间权重=1，也可加个行间高斯，这里简单处理）
                // for (int ry = -rowRadius; ry <= rowRadius; ry++)
                // {
                //     int y = y0 + ry;
                //     if ((uint)y >= (uint)H) continue;

                //     // 行间权重（可选高斯，这里给个弱衰减）
                //     float wy = rowRadius > 0 ? (float)Math.Exp(-0.5f * (ry * ry) / Math.Max(1f, rowRadius * rowRadius)) : 1f;

                //     // 该行中心列
                //     float xc = xCenter; // 也可考虑随 ry 做一点斜向偏移，这里保持不变
                //     int xC = (int)Math.Round(xc);

                //     // 横向遍历整行（也可只遍历 [xC - R, xC + R] 窗口以提速）
                //     int R = (int)Math.Ceiling(sigma * (profile == WeightProfile.Gaussian ? 3f : 1f)); // 窗口
                //     int xL = Math.Max(0, xC - R), xR = Math.Min(W - 1, xC + R);

                //     for (int x = xL; x <= xR; x++)
                //     {
                //         var c = pixels[y * W + x];
                //         if (Skip(c)) continue;
                //         int dxAbs = Math.Abs(x - xC);
                //         float wx = WtX(dxAbs);
                //         if (wx <= 0) continue;

                //         double w = wx * wy;
                //         r += (long)(c.R * w);
                //         g += (long)(c.G * w);
                //         b += (long)(c.B * w);
                //         a += (long)(c.A * w);
                //         wSum += w;
                //     }
                // }
                // 列采样
                int x0 = (int)Math.Round(xCenter);

                for (int rx = -rowRadius; rx <= rowRadius; rx++)
                {
                    int x = x0 + rx;
                    if ((uint)x >= (uint)W) continue;

                    float wx = rowRadius > 0
                        ? (float)Math.Exp(-0.5f * (rx * rx) / Math.Max(1f, rowRadius * rowRadius))
                        : 1f;

                    float yc = yCenter;
                    int yC = (int)Math.Round(yc);

                    int R = (int)Math.Ceiling(sigma * (profile == WeightProfile.Gaussian ? 3f : 1f));
                    int yT = Math.Max(0, yC - R), yB = Math.Min(H - 1, yC + R);

                    for (int y = yT; y <= yB; y++)
                    {
                        var c = pixels[y * W + x];
                        if (Skip(c)) continue;
                        int dyAbs = Math.Abs(y - yC);
                        float wy = WtX(dyAbs);      // 这里用同一权重函数对“纵向距离”加权
                        if (wy <= 0) continue;

                        double w = wy * wx;
                        r += (long)(c.R * w);
                        g += (long)(c.G * w);
                        b += (long)(c.B * w);
                        a += (long)(c.A * w);
                        wSum += w;
                    }
                }

                if (wSum <= 1e-6)
                {
                    outCols[i] = new Color(255, 255, 255, 0); // 空则给透明白或你喜欢的默认色
                }
                else
                {
                    outCols[i] = new Color(
                        (int)Math.Round(r / wSum),
                        (int)Math.Round(g / wSum),
                        (int)Math.Round(b / wSum),
                        (int)Math.Round(a / wSum)
                    );
                }
            }

            _cacheRowWeighted[key] = outCols;
            return outCols;
        }
    }
}
