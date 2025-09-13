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
        public static int SwordShaderNum = 15;
    }
    class ScallionSwordProj : ModProjectile
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Melee/ScallionSword";
        // 用静态句柄存资源：所有实例共享
        private static Asset<Texture2D> TexAsset;
        Player player => Main.player[Projectile.owner];//获取玩家
        private static float SwordShaderExtraLen = 0.8f;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                TexAsset = ModContent.Request<Texture2D>(
                    "WuDao/Content/Projectiles/Melee/ScallionSwordTail3",
                    AssetRequestMode.AsyncLoad);
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
            ProjectileID.Sets.TrailCacheLength[Type] = Vertex.SwordShaderNum;//这一项代表记录的轨迹最多能追溯到多少帧以前
            base.SetStaticDefaults();
        }
        public override void AI()//模拟"刀"的挥舞逻辑
        {
            // player.itemTime = player.itemAnimation = 3;//防止弹幕没有 趋势 玩家就又可以使用武器了
            Projectile.Center = player.Center;//绑定玩家和弹幕的位置
            Projectile.velocity = new Vector2(0, -10).RotatedBy(Projectile.rotation);//给弹幕一个速度 仅仅用于击退方向
            Projectile.rotation += 0.314f * player.direction;//弹幕每帧旋转角度18°
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
            int len = Math.Min(Projectile.oldRot.Length, ProjectileID.Sets.TrailCacheLength[Type]);
            if (len < 2) return null;

            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float radius = tex.Width * 1.9f;
            Vector2 center = Projectile.Center;
            float halfWidth = radius * 0.5f;

            float collisionPoint = 0;

            for (int i = 0; i < len - 1; i++)
            {
                // 模拟刀光的挥舞路径
                float widthFactor = 1f + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction * SwordShaderExtraLen;

                // 连续两帧的中线端点，组成一小段弧线近似
                Vector2 p0 = center + new Vector2(0f, -radius).RotatedBy(Projectile.oldRot[i]);
                Vector2 p1 = center + new Vector2(0f, -radius).RotatedBy(Projectile.oldRot[i + 1]);

                DebugDrawCapsule(p0, p1, halfWidth * widthFactor);

                // 与目标 AABB 做“线段 vs 盒子”的宽线碰撞
                if (Collision.CheckAABBvLineCollision(
                        targetHitbox.TopLeft(), targetHitbox.Size(),
                        p0, p1, // 线段
                        halfWidth * widthFactor, // 线段半宽（带厚度）
                        ref collisionPoint))
                {
                    return true;
                }
            }

            return null;
        }
        void DebugDrawCapsule(Vector2 a, Vector2 b, float halfWidth)
        {
            // 中线
            for (float t = 0; t <= 1f; t += 0.25f)
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
            for (float ang = 0; ang < MathHelper.TwoPi; ang += MathHelper.PiOver4)
            {
                Vector2 ca = a + ang.ToRotationVector2() * halfWidth;
                Dust.NewDustPerfect(ca, DustID.Smoke, Vector2.Zero, 0, Color.Gray, 0.7f).noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            GraphicsDevice gd = Main.graphics.GraphicsDevice;

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            // 可选：使用原版染料 shader
            GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(ItemID.RedDye), Projectile);
            // 开始顶点绘制
            List<Vertex> ve = new List<Vertex>();
            // 方法二：单独用一张彩色贴图 shader 颜色用白色
            Color b = Color.White;

            int len = ProjectileID.Sets.TrailCacheLength[Type];
            for (int i = 0; i < len; i++)
            {
                // 方法一：直接用双色插值做渐变
                // Color b = Color.Lerp(Color.White, new Color(10, 60, 200), (i + 1) / 9f);
                b.A = (byte)(180 * (len - i) / (float)len);
                // 刀光沿半径的放大倍数
                float tmp = 1 + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction * SwordShaderExtraLen;

                // // 刀光外圈顶点
                // ve.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -80).RotatedBy(Projectile.oldRot[i]) * tmp, new Vector3(i / (float)len, 1, 1), b));
                // // 刀光内圈顶点
                // ve.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -20).RotatedBy(Projectile.oldRot[i]) * tmp, new Vector3(i / (float)len, 0, 1), b));
                // 刀光外圈顶点
                ve.Add(new Vertex(
                    Projectile.Center - Main.screenPosition + new Vector2(0, -80).RotatedBy(Projectile.oldRot[i]) * tmp,
                    new Vector3(
                        Math.Max(len - i, len * 0.5f) / (float)len, (0.5f + Math.Min(i, len * 0.5f)) / (float)len, 1),
                    b));
                // 刀光内圈顶点
                ve.Add(new Vertex(
                    Projectile.Center - Main.screenPosition + new Vector2(0, -20).RotatedBy(Projectile.oldRot[i]) * tmp,
                    new Vector3(
                        (Math.Max(len - i, len * 0.5f) - 1.0f) / (float)len, 0, 1),
                    b));
            }

            if (ve.Count >= 3)//因为顶点需要围成一个三角形才能画出来 所以需要判顶点数>=3 否则报错
            {
                // gd.Textures[0] = TexAsset.Value;
                gd.Textures[0] = TextureAssets.Projectile[Type].Value;
                gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, ve.ToArray(), 0, ve.Count - 2);//画
            }

            //结束顶点绘制
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


            //画出这把 剑 的样子 new Vector2(0, 40) 剑柄位置
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
