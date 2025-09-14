using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace WuDao.Common.Rendering
{
    // 参数包：把“与具体武器相关的选择”一次性喂给渲染器
    public struct BladeTrailParams
    {
        public Vector2 WorldCenter;          // 世界坐标的中心（通常 Projectile.Center）
        public Func<int, float> RotAt;       // 取历史旋转：i -> oldRot[i]
        public int TrailLen;                 // 轨迹长度（与 TrailCacheLength 一致）
        public float OuterRadius;            // 外沿半径（例：80）
        public float InnerRadius;            // 内沿半径（例：20）
        public Func<int, float> HalfWidth;   // 半宽函数：i -> 半宽像素（可随i变化）
        public Func<int, Color> ColorAt;     // 颜色函数：i -> 顶点颜色
        public Func<int, Vector2> UvOuter;   // 外沿UV：i -> (u,v)
        public Func<int, Vector2> UvInner;   // 内沿UV：i -> (u,v)

        // 是否使用原版染料（GameShaders.Armor），传入 shaderId（例如 ItemID.RedDye）
        public int? ArmorDyeShaderItemId;
        // 贴图：slot0（通常遮罩/或直接用武器图）
        public Texture2D Texture0;
        // 自定义 Effect（可选）。留空则走固定管线（仅颜色/不采样自定义UV）
        public Effect Effect;
        // 是否使用加法混合
        public bool Additive;
    }

    public static class BladeTrailRenderer
    {
        // 你自己的顶点结构（保持兼容）
        public struct V : IVertexType
        {
            private static readonly VertexDeclaration Decl = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
            );
            public Vector2 Position;
            public Color Color;
            public Vector3 TexCoord; // xy=uv, z=1

            public V(Vector2 pos, Color color, Vector2 uv)
            { Position = pos; Color = color; TexCoord = new Vector3(uv, 1f); }

            public VertexDeclaration VertexDeclaration => Decl;
        }

        // 核心：构建刀光条带（TriangleStrip）
        public static void BuildStrip(List<V> verts, in BladeTrailParams p)
        {
            verts.Clear();
            int len = p.TrailLen;
            Vector2 screenOfs = Main.screenPosition;
            for (int i = 0; i < len; i++)
            {
                float rot = p.RotAt(i);
                float hw = p.HalfWidth(i); // 半宽下限避免消失
                // “半径缩放”交给 HalfWidth/或你自己的 tmp 因子，这里只管几何
                Vector2 outer = p.WorldCenter + new Vector2(0f, -p.OuterRadius).RotatedBy(rot) * hw;
                Vector2 inner = p.WorldCenter + new Vector2(0f, -p.InnerRadius).RotatedBy(rot) * hw;

                Color c = p.ColorAt(i);
                Vector2 uvO = p.UvOuter(i);
                Vector2 uvI = p.UvInner(i);

                // 注意：这里不再乘“tmp(=1+cos*dir)”去拉半径，建议把这种“宽度/形变”逻辑放入 HalfWidth/UV 里
                verts.Add(new V(outer - screenOfs, c, uvO));
                verts.Add(new V(inner - screenOfs, c, uvI));
            }
        }

        // 渲染：提交到 GPU
        public static void Draw(List<V> verts, Texture2D tex0, Effect fx, bool additive)
        {
            var gd = Main.graphics.GraphicsDevice;
            // 结束 spriteBatch，切顶点管线
            Main.spriteBatch.End();

            if (verts.Count >= 3)
            {
                gd.RasterizerState = RasterizerState.CullNone;
                gd.SamplerStates[0] = SamplerState.AnisotropicClamp;
                gd.BlendState = additive ? BlendState.Additive : BlendState.AlphaBlend;
                gd.Textures[0] = tex0;

                if (fx != null)
                {
                    // 常用：正交矩阵（像素坐标）
                    var proj = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
                    fx.Parameters["MatrixTransform"]?.SetValue(proj);
                    foreach (var pass in fx.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts.ToArray(), 0, verts.Count - 2);
                    }
                }
                else
                {
                    // 无 Effect 时，走固定管线（注意：此时自定义UV不会被用来自采样）
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts.ToArray(), 0, verts.Count - 2);
                }
            }
            
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        // 一条龙：可选地套“原版染料”后再画
        public static void Render(ref BladeTrailParams p, List<V> scratch)
        {
            if (p.Texture0 == null || p.TrailLen < 2) return;

            BuildStrip(scratch, p);

            // test 放到后面行不行呢？可选：套原版染料（Armor Shader）
            if (p.ArmorDyeShaderItemId.HasValue)
                GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(p.ArmorDyeShaderItemId.Value), null);

            Draw(scratch, p.Texture0, p.Effect, p.Additive);
        }
    }

    public static class BladeTrailCollision
    {
        public static bool CheckCollision(Vector2 center, float[] oldRot, int len,
                                          float outerRadius, float halfWidthBase,
                                          int playerDir, float extraLen,
                                          Rectangle targetHitbox)
        {
            if (len < 2) return false;
            float collisionPoint = 0f;

            for (int i = 0; i < len - 1; i++)
            {
                float wf = 1f + (float)System.Math.Cos(oldRot[i] - MathHelper.PiOver2) * playerDir * extraLen;
                wf = System.Math.Max(0.2f, wf);

                Vector2 p0 = center + new Vector2(0f, -outerRadius).RotatedBy(oldRot[i]);
                Vector2 p1 = center + new Vector2(0f, -outerRadius).RotatedBy(oldRot[i + 1]);

                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                                                     p0, p1, halfWidthBase * wf, ref collisionPoint))
                    return true;
            }
            return false;
        }
    }
}
