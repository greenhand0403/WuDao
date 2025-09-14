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

    public static class TrailColorSampler
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
}
