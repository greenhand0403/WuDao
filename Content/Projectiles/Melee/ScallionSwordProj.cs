using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Melee
{
    public struct Vertex : IVertexType
    {
        private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[3]
        {
            // 偏移地址分别对应 Position(2个float对应8Byte)、Color(4个Byte)、TexCoord(3个float对应12Byte) 的字节大小
            new VertexElement(0,VertexElementFormat.Vector2,VertexElementUsage.Position,0),
            new VertexElement(8,VertexElementFormat.Color,VertexElementUsage.Color,0),
            new VertexElement(12,VertexElementFormat.Vector3,VertexElementUsage.TextureCoordinate,0)
        });
        public Vector2 Position;
        public Color Color;
        public Vector3 TexCoord;
        public Vertex(Vector2 position, Vector3 texCoord, Color color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }
        public VertexDeclaration VertexDeclaration
        {
            get => _vertexDeclaration;
        }
    }
    class ScallionSwordProj : ModProjectile
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Melee/ScallionSword";
        // 用静态句柄存资源：所有实例共享
        private static Asset<Texture2D> TexAsset;
        private static Color avgColor;
        public static Vector3 RgbToHsl(Vector3 rgb)
        {
            float max = Math.Max(rgb.X, Math.Max(rgb.Y, rgb.Z));
            float min = Math.Min(rgb.X, Math.Min(rgb.Y, rgb.Z));
            float h = 0f, s, l = (max + min) / 2f;

            if (max == min)
            {
                h = s = 0f; // 灰色
            }
            else
            {
                float d = max - min;
                s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
                if (max == rgb.X)
                    h = (rgb.Y - rgb.Z) / d + (rgb.Y < rgb.Z ? 6f : 0f);
                else if (max == rgb.Y)
                    h = (rgb.Z - rgb.X) / d + 2f;
                else
                    h = (rgb.X - rgb.Y) / d + 4f;
                h /= 6f;
            }
            return new Vector3(h, s, l); // H[0..1], S[0..1], L[0..1]
        }

        public static Vector3 HslToRgb(Vector3 hsl)
        {
            float r, g, b;
            float h = hsl.X, s = hsl.Y, l = hsl.Z;

            if (s == 0)
            {
                r = g = b = l; // 灰
            }
            else
            {
                Func<float, float, float, float> HueToRgb = (p, q, t) =>
                {
                    if (t < 0) t += 1f;
                    if (t > 1) t -= 1f;
                    if (t < 1f / 6f) return p + (q - p) * 6f * t;
                    if (t < 1f / 2f) return q;
                    if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
                    return p;
                };

                float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
                float p = 2f * l - q;
                r = HueToRgb(p, q, h + 1f / 3f);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1f / 3f);
            }
            return new Vector3(r, g, b);
        }

        public override void Load()
        {
            if (!Main.dedServ)
            {
                TexAsset = ModContent.Request<Texture2D>(
                    "WuDao/Content/Projectiles/Melee/ScallionSwordTail",
                    AssetRequestMode.AsyncLoad);

                // 取整张贴图像素
                Texture2D tex = TexAsset.Value;
                Color[] data = new Color[tex.Width * tex.Height];
                tex.GetData(data);

                int count = Math.Min(tex.Width, tex.Height);
                Vector3 sum = Vector3.Zero; // 累加到 float3
                float totalWeight = 0f;

                for (int i = 0; i < count; i++)
                {
                    // 对角线索引 (i,i)
                    Color c = data[i + i * tex.Width];

                    // 转换到 [0,1] 浮点
                    Vector3 rgb = c.ToVector3();

                    // 权重（你可以用位置权重，例如越靠近中心越大）
                    float w = 1f;
                    // 或者: float w = 1f - Math.Abs((i - count/2f) / (count/2f));

                    sum += rgb * w;
                    totalWeight += w;
                }

                Vector3 avgRgb = sum / totalWeight;
                avgColor = new Color(avgRgb);
            }
        }
        public override void Unload()
        {
            TexAsset = null;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.friendly = true;//友方弹幕
            Projectile.tileCollide = false;//穿墙
            Projectile.aiStyle = -1;//不使用原版AI
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;//无限穿透
            Projectile.ignoreWater = true;//无视液体

            //或者让它不死 一直转(
            Projectile.timeLeft = 10;//弹幕 趋势 的时间
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void SetStaticDefaults()//以下照抄
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;//这一项赋值2可以记录运动轨迹和方向（用于制作拖尾）
            ProjectileID.Sets.TrailCacheLength[Type] = 9;//这一项代表记录的轨迹最多能追溯到多少帧以前
            base.SetStaticDefaults();
        }

        Player player => Main.player[Projectile.owner];//获取玩家
        public override void AI()//模拟"刀"的挥舞逻辑
        {
            // player.itemTime = player.itemAnimation = 3;//防止弹幕没有 趋势 玩家就又可以使用武器了
            Projectile.Center = player.Center;//绑定玩家和弹幕的位置
            Projectile.velocity = new Vector2(0, -10).RotatedBy(Projectile.rotation);//给弹幕一个速度 仅仅用于击退方向
            Projectile.rotation += 0.32f * player.direction;//弹幕每帧旋转角度
            player.heldProj = Projectile.whoAmI;//使弹幕的贴图画出来后 夹 在角色的身体和手之间

            //以下为升级内容
            /*
            if (Projectile.rotation > MathHelper.Pi)
                Projectile.rotation = -MathHelper.Pi;
            if (Projectile.rotation < -MathHelper.Pi)
                Projectile.rotation = MathHelper.Pi;
            */
            // if (player.controlUseItem)
                // Projectile.timeLeft = 2;//让弹幕一直转圈圈的方法之一
        }
        public override bool ShouldUpdatePosition()
        {
            return false;//让弹幕位置不受速度影响
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 用“刀背-刀刃”两条边之间的“中线”近似刀锋路径，并且给它一个宽度
            // 你绘制时用的是 oldRot[i] 生成两条边：-80 与 -20，碰撞我们取中间半径 ~ -50
            // 宽度取 32~40 像素（可按贴图调）

            int len = Math.Min(Projectile.oldRot.Length, 9);
            if (len < 2) return false;

            float radius = 50f;   // 中线半径（介于 20~80 之间）
            float halfWidth = 18f; // “线段的半宽”，越大越宽

            Vector2 center = Projectile.Center;

            bool hit = false;
            float collisionPoint = 0;

            for (int i = 0; i < len - 1; i++)
            {
                // 连续两帧的中线端点，组成一小段弧线近似
                Vector2 p0 = center + new Vector2(0f, -radius).RotatedBy(Projectile.oldRot[i]);
                Vector2 p1 = center + new Vector2(0f, -radius).RotatedBy(Projectile.oldRot[i + 1]);

                DebugDrawCapsule(p0, p1, halfWidth);

                // 与目标 AABB 做“线段 vs 盒子”的宽线碰撞
                if (Collision.CheckAABBvLineCollision(
                        targetHitbox.TopLeft(), targetHitbox.Size(),
                        p0, p1, // 线段
                        halfWidth, // 线段半宽（带厚度）
                        ref collisionPoint))
                {
                    hit = true;
                    break;
                }
            }

            return hit;
        }
        void DebugDrawCapsule(Vector2 a, Vector2 b, float halfWidth)
        {
            // 中线
            for (float t = 0; t <= 1f; t += 0.05f)
            {
                Vector2 p = Vector2.Lerp(a, b, t);
                Dust.NewDustPerfect(p, DustID.GoldFlame, Vector2.Zero, 0, Color.White, 1.1f).noGravity = true;

                // 两条“平行边”（可视化半宽）
                Vector2 v = b - a;
                Vector2 n = v.RotatedBy(MathHelper.PiOver2);
                n.Normalize();
                Vector2 pL = p + n * halfWidth;
                Vector2 pR = p - n * halfWidth;
                Dust.NewDustPerfect(pL, DustID.Smoke, Vector2.Zero, 0, Color.LightGray, 0.8f).noGravity = true;
                Dust.NewDustPerfect(pR, DustID.Smoke, Vector2.Zero, 0, Color.LightGray, 0.8f).noGravity = true;
            }

            // 两端圆帽
            for (float ang = 0; ang < MathHelper.TwoPi; ang += 0.2f)
            {
                Vector2 ca = a + ang.ToRotationVector2() * halfWidth;
                Vector2 cb = b + ang.ToRotationVector2() * halfWidth;
                Dust.NewDustPerfect(ca, DustID.Smoke, Vector2.Zero, 0, Color.Gray, 0.7f).noGravity = true;
                Dust.NewDustPerfect(cb, DustID.Smoke, Vector2.Zero, 0, Color.Gray, 0.7f).noGravity = true;
            }
        }
        // 蓝白水之呼吸的配色 t: 0..1 沿条带从柄到刃
        Color SampleBlade(float t)
        {
            if (t < 0.35f)
            { // 白
                float u = t / 0.35f;
                return Color.Lerp(new Color(255, 255, 255), new Color(190, 230, 255), u);
            }
            else if (t < 0.75f)
            { // 浅蓝
                float u = (t - 0.35f) / 0.40f;
                return Color.Lerp(new Color(190, 230, 255), new Color(80, 160, 255), u);
            }
            else
            { // 深蓝
                float u = (t - 0.75f) / 0.25f;
                return Color.Lerp(new Color(80, 160, 255), new Color(10, 60, 200), u);
            }
            // 方法二：用 hsl 做渐变色
            // Vector3 baseHsl = RgbToHsl(avgRgb);
            // 从基色出发，修改亮度和饱和度
            // float h = baseHsl.X; // 固定色相
            // float s = MathHelper.Lerp(0.2f, baseHsl.Y, t); // 从淡到浓
            // float l = MathHelper.Lerp(0.9f, baseHsl.Z * 0.6f, t); // 从亮到暗
            // Vector3 rgb = HslToRgb(new Vector3(h, s, l));
            // return new Color(rgb);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            //缩写这俩 我懒得在后面打长长的东西
            SpriteBatch sb = Main.spriteBatch;
            GraphicsDevice gd = Main.graphics.GraphicsDevice;

            //end 和 begin里和顶点的东西建议照抄 然后慢慢理解

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            // 使用原版染料 shader
            // GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(ItemID.RedDye), Projectile);
            //开始顶点绘制

            List<Vertex> ve = new List<Vertex>();

            for (int i = 0; i < 9; i++)
            {
                // 更换水之呼吸的配色 方法二：hsl 的情况下 除以8会如何呢？
                Color b = SampleBlade(i / 9f);
                // 方法三：单独用一张彩色贴图 shader 颜色用白色
                // Color b = Color.White;
                // 直接用双色插值做渐变
                // Color b = Color.Lerp(Color.Red, Color.Blue, i / 9f);

                ve.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -80).RotatedBy(Projectile.oldRot[i]) * (1 + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction),
                          new Vector3(i / 9f, 1, 1),
                          b));
                ve.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -20).RotatedBy(Projectile.oldRot[i]) * (1 + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction),
                          new Vector3(i / 9f, 0, 1),
                          b));
            }

            if (ve.Count >= 3)//因为顶点需要围成一个三角形才能画出来 所以需要判顶点数>=3 否则报错
            {
                gd.Textures[0] = TexAsset.Value;
                gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, ve.ToArray(), 0, ve.Count - 2);//画
            }

            //结束顶点绘制
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


            //画出这把 剑 的样子
            Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value,
                             Projectile.Center - Main.screenPosition,
                             null,
                             lightColor,
                             Projectile.rotation - MathHelper.PiOver4,
                             new Vector2(0, 40),
                             1.5f,
                             SpriteEffects.None,
                             0);

            return false;//让弹幕不画原来的样子
        }
    }
}
