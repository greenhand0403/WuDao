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
using WuDao.Common.Rendering;

namespace WuDao.Content.Projectiles.Melee
{
    class ScallionSwordProj : ModProjectile
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Melee/ScallionSword";
        // 用静态句柄存资源：所有实例共享
        public static Asset<Texture2D> SwordTailTexAsset;
        Player player => Main.player[Projectile.owner];//获取玩家
        // 挥剑时横向例如朝右额外伸出的倍数
        private static float SwordShaderExtraLen = 0.8f;
        private static readonly List<BladeTrailRenderer.V> _trailVerts = new();
        public override void Load()
        {
            if (!Main.dedServ)
            {
                SwordTailTexAsset = ModContent.Request<Texture2D>(
                    "WuDao/Content/Projectiles/Melee/ScallionSwordTail3",
                    AssetRequestMode.AsyncLoad);
            }
        }
        public override void Unload()
        {
            SwordTailTexAsset = null;
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
            ProjectileID.Sets.TrailCacheLength[Type] = 15;//这一项代表记录的轨迹最多能追溯到多少帧以前
            base.SetStaticDefaults();
        }
        public override void AI()//模拟"刀"的挥舞逻辑
        {
            // 初始化初始旧旋转位置
            // if (Projectile.localAI[0] == 0f)
            // {
            //     // 起始挥舞角度
            //     Projectile.rotation = -MathHelper.PiOver2 * player.direction;
            //     for (int i = 0; i < 15; i++)
            //     {
            //         Projectile.oldRot[i] = Projectile.rotation;
            //     }
            //     Projectile.localAI[0] = 1f;
            // }
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

            // 刀光碰撞体中心与玩家中心的半径
            float radius = player.itemWidth * 1.9f;
            Vector2 center = Projectile.Center;
            float halfWidth = radius * 0.5f;

            float collisionPoint = 0;

            for (int i = 0; i < len - 1; i++)
            {
                // 模拟刀光的挥舞路径，需要与初始挥舞角度配合
                float widthFactor = 1f + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction * SwordShaderExtraLen;

                // 连续两帧的中线端点，组成一小段弧线近似
                Vector2 p0 = center + new Vector2(0f, -radius).RotatedBy(Projectile.oldRot[i]);
                Vector2 p1 = center + new Vector2(0f, -radius).RotatedBy(Projectile.oldRot[i + 1]);

                // DebugDrawCapsule(p0, p1, halfWidth * widthFactor);

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
            // 用拖尾的帧数进行顶点绘制刀光
            int len = ProjectileID.Sets.TrailCacheLength[Type];
            if (len < 2) return false;

            // —— 组装参数 —— //
            BladeTrailParams p = new BladeTrailParams
            {
                WorldCenter = Projectile.Center,
                RotAt = (i) => Projectile.oldRot[i],
                TrailLen = len,
                OuterRadius = 1.67f * player.itemWidth,
                InnerRadius = 0.42f * player.itemWidth,

                // 半宽：用你的“1 + cos * dir * ExtraLen”，并给下限避免外圈消失
                // 需要自己根据初始旋转角度推导刀光前向延长距离的缩放关系 水平刀光=abs+sin 垂直刀光=cos
                // 也可以根据弹幕存活时间、物品当前使用时间来决定刀光延长的距离
                HalfWidth = (i) =>
                {
                    // 水平刀光缩放
                    // float wf = 1f + (float)Math.Abs(Math.Sin(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction) * SwordShaderExtraLen;
                    float wf = 1f + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction * SwordShaderExtraLen;
                    return wf;
                },

                // 颜色：可改成任何函数（或直接 Color.White，用贴图色）
                ColorAt = (i) =>
                {
                    // 配合顶点遮罩使用
                    // var c = Color.Lerp(Color.OrangeRed, new Color(10, 60, 200), (i + 1) / (float)len);
                    // 适合直接用顶点颜色
                    var c = Color.White;
                    c.A = (byte)(180 * (len - i) / (float)len);
                    return c;
                },

                // 经典三角形刀光 UV 从 外圈(0,1) 内圈(0,0) 到 外圈(1,1) 内圈(1,0)
                // UvOuter = (i) => new Vector2(i / (float)(len - 1), 1f),
                // UvInner = (i) => new Vector2(i / (float)(len - 1), 0f),
                // 直接从物品贴图中取对角线的顶点颜色 从 外圈(1,0.5+1/len) 内圈(1-1/len,0) 到 外圈(0.5,0.5+1/len) 内圈(0.5-1/len,0)
                UvOuter = (i) => new Vector2(Math.Max(len - i, len * 0.5f) / (float)len, (0.5f + Math.Min(i, len * 0.5f)) / (float)len),
                UvInner = (i) => new Vector2((Math.Max(len - i, len * 0.5f) - 1.0f) / (float)len, 0),

                // 原版染料：示例 RedDye；若不需要，设为 null
                // ArmorDyeShaderItemId = null,
                ArmorDyeShaderItemId = ItemID.RedDye,

                // 贴图/着色器：可用遮罩/或直接 Projectile 贴图；自定义 Effect 可空
                // 使用额外的顶点贴图作为遮罩
                // Texture0 = SwordTailTexAsset.Value,
                // 直接用武器贴图做顶点绘制
                Texture0 = TextureAssets.Projectile[Type].Value,

                Effect = null,  // 若你写了 BladeTrail.fx，就换成那个 Effect
                Additive = true,
            };

            // —— 渲染 —— //
            BladeTrailRenderer.Render(ref p, _trailVerts);
            //画出这把 剑 的样子 new Vector2(0, 40) 剑柄位置
            Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value,
                             Projectile.Center - Main.screenPosition,
                             null,
                             lightColor,
                             Projectile.rotation - MathHelper.PiOver4,
                             new Vector2(0, player.itemHeight),
                             1.5f,
                             SpriteEffects.None,
                             0);

            return false;//让弹幕不画原来的样子
        }
    }
}
