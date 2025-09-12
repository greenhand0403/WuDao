using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Melee
{
    public struct Vertex : IVertexType
    {
        private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[3]
        {
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
        public override void Load()
        {
            if (!Main.dedServ)
            {
                TexAsset = ModContent.Request<Texture2D>(
                    "WuDao/Content/Projectiles/Melee/ScallionSwordTail",
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
            base.SetDefaults();
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
        public override bool PreDraw(ref Color lightColor)
        {
            //缩写这俩 我懒得在后面打长长的东西
            SpriteBatch sb = Main.spriteBatch;
            GraphicsDevice gd = Main.graphics.GraphicsDevice;

            //end 和 begin里和顶点的东西建议照抄 然后慢慢理解

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            //开始顶点绘制

            List<Vertex> ve = new List<Vertex>();

            for (int i = 0; i < 9; i++)
            {
                Color b = Color.Lerp(Color.Red, Color.Blue, i / 9f);

                //存顶点																										从这一—————————————到这里都是乱弄的 你可以随便改改数据看看能发生什么
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
