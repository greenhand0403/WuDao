using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Melee
{
    /// <summary>
    /// 170x170 x4（竖排）刀光的“持有型近战”弹幕
    /// 贴图：SteelSlash.png（尺寸 170x680，默认朝右）
    /// </summary>
    public class SteelBroadSwordProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4; // 竖排4帧（示例会把不同pass用到第0/3帧）
            ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
        }

        public override void SetDefaults()
        {
            // 碰撞盒无所谓（我们使用自定义碰撞），保持小即可
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.ownerHitCheckDistance = 300f;
            Projectile.usesOwnerMeleeHitCD = true;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;

            Projectile.aiStyle = -1;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            // 约定：ai[0]=方向(+1/-1，含重力翻转)；ai[1]=最大时长；ai[2]=近战缩放
            Projectile.localAI[0]++;                           // 当前存活时间

            Player player = Main.player[Projectile.owner];

            float t = Projectile.localAI[0] / Projectile.ai[1]; // 0..1
            float dir = Projectile.ai[0];
            float velRot = Projectile.velocity.ToRotation();

            // 精确复刻示例的旋转表达式（不要额外 + π）
            float rot = MathHelper.Pi * dir * t + velRot + dir * MathHelper.Pi + player.fullRotation;
            Projectile.rotation = rot;

            // 位置 / 缩放也按示例：中心在玩家手前一点，逐渐放大
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity;
            float scaleMulti = 0.5f;  // 略小于 Excalibur 
            float scaleAdder = 0.95f;
            Projectile.scale = (scaleAdder + t * scaleMulti) * Projectile.ai[2];

            // Here we spawn some dust inside the arc of the swing.
            float dustRotation = Projectile.rotation + Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7f;
            Vector2 dustPosition = Projectile.Center + dustRotation.ToRotationVector2() * 84f * Projectile.scale;
            Vector2 dustVelocity = (dustRotation + Projectile.ai[0] * MathHelper.PiOver2).ToRotationVector2();
            if (Main.rand.NextFloat() * 2f < Projectile.Opacity)
            {
                // Original Excalibur color: Color.Gold, Color.White
                Color dustColor = Color.Lerp(Color.SkyBlue, Color.White, Main.rand.NextFloat() * 0.3f);
                Dust coloredDust = Dust.NewDustPerfect(Projectile.Center + dustRotation.ToRotationVector2() * (Main.rand.NextFloat() * 80f * Projectile.scale + 20f * Projectile.scale), DustID.FireworksRGB, dustVelocity * 1f, 100, dustColor, 0.4f);
                coloredDust.fadeIn = 0.4f + Main.rand.NextFloat() * 0.15f;
                coloredDust.noGravity = true;
            }

            if (Projectile.localAI[0] >= Projectile.ai[1])
                Projectile.Kill();

            // 可选：药水附魔视觉（与示例一致）
            for (float i = -MathHelper.PiOver4; i <= MathHelper.PiOver4; i += MathHelper.PiOver2)
            {
                Rectangle rect = Utils.CenteredRectangle(
                    Projectile.Center + (Projectile.rotation + i).ToRotationVector2() * 70f * Projectile.scale,
                    new Vector2(60f * Projectile.scale, 60f * Projectile.scale)
                );
                Projectile.EmitEnchantmentVisualsAt(rect.TopLeft(), rect.Width, rect.Height);
            }

            // 让玩家手臂随挥砍摆动（示例同款）
            t = Projectile.localAI[0] / Projectile.ai[1]; // 0..1
            float armRot = Projectile.rotation - MathHelper.PiOver2 * Projectile.ai[0]; // 贴合刀身
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);

            // 维持持有态
            player.heldProj = Projectile.whoAmI;
        }

        // 复制示例的“锥形”命中逻辑，贴图/视觉与判定同步
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float coneLength = 75f * Projectile.scale;
            float collisionRotation = MathHelper.Pi * 2f / 25f * Projectile.ai[0];
            float maximumAngle = MathHelper.Pi / 5;
            float coneRotation = Projectile.rotation + collisionRotation;

            if (targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation, maximumAngle))
                return true;

            float backSwing = Utils.Remap(Projectile.localAI[0], Projectile.ai[1] * 0.3f, Projectile.ai[1] * 0.5f, 1f, 0f);
            if (backSwing > 0f)
            {
                float coneRotation2 = coneRotation - MathHelper.PiOver4 * Projectile.ai[0] * backSwing;
                if (targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation2, maximumAngle))
                    return true;
            }
            return false;
        }

        public override void CutTiles()
        {
            Vector2 a = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 48f * Projectile.scale;
            Vector2 b = (Projectile.rotation + MathHelper.PiOver4).ToRotationVector2() * 48f * Projectile.scale;
            Utils.PlotTileLine(Projectile.Center + a, Projectile.Center + b, 48f * Projectile.scale, DelegateMethods.CutTiles);
        }

        // 关键：绘制严格沿用示例（中心原点 + 仅纵向翻转）
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            // 帧配置：0=主体；3=细线高光（与你的贴图约定一致）
            int frames = Main.projFrames[Type];
            int fh = tex.Height / frames;
            Rectangle frameMain = new Rectangle(0, 0 * fh, tex.Width, fh);
            Rectangle frameLine = new Rectangle(0, 3 * fh, tex.Width, fh);

            Vector2 origin = new Vector2(frameMain.Width * 0.5f, fh * 0.5f);
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float scale = Projectile.scale * 1.06f;

            SpriteEffects fx = (Projectile.ai[0] >= 0f) ? SpriteEffects.None : SpriteEffects.FlipVertically;

            float t = Projectile.localAI[0] / Projectile.ai[1];
            float lerp = Utils.Remap(t, 0f, 0.6f, 0f, 1f) * Utils.Remap(t, 0.6f, 1f, 1f, 0f);

            // ===== 三套配色 =====
            int palette = (int)MathHelper.Clamp(Projectile.localAI[1], 0, 2);
            Color c_back, c_mid, c_front;

            if (palette == 0)
            {         // 铜：暖金橙
                c_back = new Color(200, 120, 60);
                c_mid = new Color(255, 190, 120);
                c_front = new Color(255, 230, 180);
            }
            else if (palette == 2)
            {    // 银：冷白蓝
                c_back = new Color(120, 160, 200);
                c_mid = new Color(185, 225, 255);
                c_front = new Color(230, 245, 255);
            }
            else
            {                      // 铁(默认)：偏钢蓝
                c_back = new Color(60, 160, 180);
                c_mid = new Color(80, 255, 255);
                c_front = new Color(150, 240, 255);
            }

            // —— 自发光：不乘环境光，夜晚也清晰 ——
            float w = 1f;

            // ===== 主体三层（关键：确保“剑身”可见，不再只剩弧线）=====
            Main.EntitySpriteDraw(tex, pos, frameMain, c_back * (0.90f * lerp * w), Projectile.rotation + Projectile.ai[0] * (-MathHelper.PiOver4) * (1f - t), origin, scale * 1.00f, fx, 0);
            Main.EntitySpriteDraw(tex, pos, frameMain, c_mid * (0.45f * lerp * w), Projectile.rotation, origin, scale * 0.98f, fx, 0);
            Main.EntitySpriteDraw(tex, pos, frameMain, c_front * (0.65f * lerp * w), Projectile.rotation, origin, scale * 0.95f, fx, 0);

            // ===== 细线高光（第3帧）=====
            Main.EntitySpriteDraw(tex, pos, frameLine, Color.White * (0.60f * lerp), Projectile.rotation + Projectile.ai[0] * 0.01f, origin, scale * 1.00f, fx, 0);
            Main.EntitySpriteDraw(tex, pos, frameLine, Color.White * (0.50f * lerp), Projectile.rotation + Projectile.ai[0] * -0.05f, origin, scale * 0.82f, fx, 0);
            Main.EntitySpriteDraw(tex, pos, frameLine, Color.White * (0.40f * lerp), Projectile.rotation + Projectile.ai[0] * -0.10f, origin, scale * 0.64f, fx, 0);

            return false;
        }
    }
}
