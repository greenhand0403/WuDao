using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using WuDao.Common;

namespace WuDao.Content.Projectiles.Summon
{
    /// <summary>
    /// 电刑：特殊雷电鞭
    /// 
    /// 设计目标：
    /// 1. 仍然使用原版 Whip AI 负责玩家挥鞭生命周期、持有状态、命中时机等。
    /// 2. 不再使用 FillWhipControlPoints 的平滑鞭子曲线作为视觉骨架。
    /// 3. 每帧自己生成“折线式闪电鞭骨架”，让鞭子本体就是一串闪电折线。
    /// 4. PreDraw 按折线点列画闪电；Colliding 按折线点列做线段碰撞。
    /// </summary>
    public class ElectrocutionProj : ModProjectile
    {
        /// <summary>
        /// 这就是“电刑鞭”的真正骨架。
        /// 每帧根据挥鞭进度，重新生成一串折线点。
        /// </summary>
        private readonly List<Vector2> _electroWhipPoints = new();

        // ===== 可调参数 =====

        /// <summary> 鞭子的折线段数。越大越长、越细密。 </summary>
        private const int SegmentCount = 20;

        /// <summary> 鞭子的最大长度系数。 </summary>
        private const float RangeMultiplier = 1.20f;

        /// <summary> 用于命中检测的闪电线宽。 </summary>
        private const float CollisionLineWidth = 32f;

        /// <summary> 整体弯曲强度。值越大，鞭子越有挥舞弧度。 </summary>
        private const float ArcAmplitude = 36f;

        /// <summary> 折线摆动强度。值越大，越像雷蛇乱窜。 </summary>
        private const float ZigzagAmplitude = 32f;

        /// <summary> 鞭尖额外前探长度。 </summary>
        private const float TipExtraLength = 48f;

        /// <summary> 一整次挥鞭的最大视觉长度基值。 </summary>
        private const float MaxWhipLength = 420f;
        /// <summary>
        /// 距离手部多远以内完全不抖动（像素）。
        /// </summary>
        private const float JitterDeadLength = 16f;

        /// <summary>
        /// 经过多长距离后，抖动提升到完整强度（像素）。
        /// </summary>
        private const float JitterRampLength = 64f;
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BoneWhip;
        // 这里只是占位，正常绘制会在 PreDraw 里 return false 自己画掉。

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = SegmentCount;
            Projectile.WhipSettings.RangeMultiplier = RangeMultiplier;
        }

        public override void PostAI()
        {
            GenerateElectroWhipPoints();
            AddLightningLight();
            SpawnTipDust();
        }

        /// <summary>
        /// 自行生成“折线闪电鞭骨架”。
        /// 
        /// 核心思想：
        /// - 原版 whip AI 负责整体挥鞭生命周期
        /// - 我们根据 ai[0] / timeToFlyOut 得到当前挥鞭进度
        /// - 再用“主方向 + 挥舞弧度 + 交替折线偏移 + 少量随机偏移”
        ///   生成一串更像闪电珠弧的折线骨架
        /// </summary>
        private void GenerateElectroWhipPoints()
        {
            _electroWhipPoints.Clear();

            Player owner = Main.player[Projectile.owner];
            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out _, out _);

            // 当前挥鞭进度：0 -> 1
            float swingProgress = Projectile.ai[0] / timeToFlyOut;
            swingProgress = MathHelper.Clamp(swingProgress, 0f, 1f);

            // 玩家手臂位置作为鞭子起点
            Vector2 handPosition = Main.GetPlayerArmPosition(Projectile);

            // 初始朝向：使用发射时的 velocity 作为主轴方向
            Vector2 aimDirection = Projectile.velocity;
            if (aimDirection == Vector2.Zero)
                aimDirection = new Vector2(owner.direction, 0f);

            aimDirection.Normalize();

            // 用一个“左右摆动的角度偏移”来模拟挥鞭轨迹
            // 这样鞭子会随着挥舞过程扫出弧线，而不是死板地固定指向前方
            float swingAngleOffset = MathHelper.Lerp(-1.15f, 1.05f, swingProgress) * owner.direction;
            Vector2 whipForward = aimDirection.RotatedBy(swingAngleOffset);

            // 法线方向，用于制造整体挥舞弧度 + 闪电折线偏移
            Vector2 whipNormal = whipForward.RotatedBy(MathHelper.PiOver2);

            // 长度包络：前半程伸长，后半程收回
            float extension = (float)Math.Sin(swingProgress * Math.PI);
            float currentWhipLength = MaxWhipLength * Projectile.WhipSettings.RangeMultiplier * extension;

            // 太短时不必继续
            if (currentWhipLength <= 4f)
            {
                _electroWhipPoints.Add(handPosition);
                return;
            }

            // 随机种子：每帧会变，但同一帧保持稳定
            int seed = Projectile.whoAmI * 997 + (int)Main.GameUpdateCount * 131;
            UnifiedRandom random = new UnifiedRandom(seed);

            _electroWhipPoints.Add(handPosition);

            int zigzagPhase = (int)(Main.GameUpdateCount + Projectile.whoAmI) % 2;
            // 用“离散折段”而不是平滑曲线去构造整条鞭子
            for (int segmentIndex = 1; segmentIndex <= SegmentCount; segmentIndex++)
            {
                float t = segmentIndex / (float)SegmentCount;

                // 沿主方向前进的基础位置
                Vector2 basePoint = handPosition + whipForward * currentWhipLength * t;

                // 整体挥舞弧度：中段最明显，首尾最弱
                float arcOffset = (float)Math.Sin(t * Math.PI) * ArcAmplitude * extension;

                // 折线摆动：鞭子根部不抖动，然后逐渐过渡到完全抖动，最后使用完全抖动的强度
                float distanceFromHand = Vector2.Distance(handPosition, basePoint);
                float jitterFactor = Utils.GetLerpValue(
                    JitterDeadLength,
                    JitterDeadLength + JitterRampLength,
                    distanceFromHand,
                    true);

                float zigzagStrength = ZigzagAmplitude * jitterFactor * extension;

                // 固定折线只保留一部分骨架感
                // float alternatingOffset = (segmentIndex % 2 == 0 ? -1f : 1f) * zigzagStrength * 0.45f;
                float alternatingOffset = ((segmentIndex + zigzagPhase) % 2 == 0 ? -1f : 1f) * zigzagStrength * 0.45f;
                // 随机扰动改大，让每帧形状明显不同
                float randomOffset = (random.NextFloat() * 2f - 1f) * zigzagStrength * 0.95f;

                float forwardJitter = (random.NextFloat() * 2f - 1f) * zigzagStrength * 0.25f;

                Vector2 finalPoint = basePoint
                    + whipNormal * (arcOffset + alternatingOffset + randomOffset)
                    + whipForward * forwardJitter;

                _electroWhipPoints.Add(finalPoint);
            }

            // 鞭尖向前额外拉长一点，形成更尖锐的电击头
            if (_electroWhipPoints.Count >= 2)
            {
                int lastIndex = _electroWhipPoints.Count - 1;
                Vector2 tipDirection = _electroWhipPoints[lastIndex] - _electroWhipPoints[lastIndex - 1];

                if (tipDirection != Vector2.Zero)
                {
                    tipDirection.Normalize();
                    _electroWhipPoints[lastIndex] += tipDirection * TipExtraLength * extension;
                }
            }
        }

        /// <summary>
        /// 让整条电鞭沿线发出和原版珠弧类似的青白色雷光。
        /// </summary>
        private void AddLightningLight()
        {
            if (_electroWhipPoints.Count == 0)
                return;

            // 隔几个点打一盏灯，避免太重
            for (int i = 0; i < _electroWhipPoints.Count; i += 3)
            {
                Lighting.AddLight(_electroWhipPoints[i], 0.30f, 0.45f, 0.50f);
            }
        }

        /// <summary>
        /// 在鞭尖喷一点电火花，让末端更有“击打感”。
        /// </summary>
        private void SpawnTipDust()
        {
            if (_electroWhipPoints.Count < 2)
                return;

            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out _, out _);

            float swingProgress = Projectile.ai[0] / timeToFlyOut;
            swingProgress = MathHelper.Clamp(swingProgress, 0f, 1f);

            // 在中前段更容易出火花
            float strength =
                Utils.GetLerpValue(0.15f, 0.55f, swingProgress, true) *
                Utils.GetLerpValue(0.95f, 0.65f, swingProgress, true);

            if (strength <= 0.1f || Main.rand.NextFloat() >= strength * 0.55f)
                return;

            Vector2 tip = _electroWhipPoints[^1];
            Vector2 previous = _electroWhipPoints[^2];
            Vector2 direction = tip - previous;

            if (direction != Vector2.Zero)
                direction.Normalize();

            Rectangle area = Utils.CenteredRectangle(tip, new Vector2(20f, 20f));

            int dustIndex = Dust.NewDust(area.TopLeft(), area.Width, area.Height, DustID.Electric, 0f, 0f, 0, default, 1.05f);
            Dust dust = Main.dust[dustIndex];
            dust.noGravity = true;
            dust.velocity *= 0.2f;
            dust.velocity += direction * 1.9f;
        }

        // public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        // {
        //     if (_electroWhipPoints.Count < 2)
        //         return false;

        //     float collisionPoint = 0f;

        //     // 判定宽度不要太细，至少要明显大于你现在那条闪电线视觉宽度
        //     float lineWidth = CollisionLineWidth * Projectile.scale;

        //     for (int i = 0; i < _electroWhipPoints.Count - 1; i++)
        //     {
        //         Vector2 start = _electroWhipPoints[i];
        //         Vector2 end = _electroWhipPoints[i + 1];

        //         // 1. 先做“粗线段判定”
        //         if (Collision.CheckAABBvLineCollision(
        //             targetHitbox.TopLeft(),
        //             targetHitbox.Size(),
        //             start,
        //             end,
        //             lineWidth,
        //             ref collisionPoint))
        //         {
        //             return true;
        //         }

        //         // 2. 再补一个“段点矩形判定”，更接近原版鞭子的宽体感
        //         Rectangle segmentHitbox = Utils.CenteredRectangle(start, new Vector2(lineWidth, lineWidth));
        //         if (segmentHitbox.Intersects(targetHitbox))
        //         {
        //             return true;
        //         }
        //     }

        //     // 最后一个点也补一下
        //     Rectangle tipHitbox = Utils.CenteredRectangle(_electroWhipPoints[^1], new Vector2(lineWidth, lineWidth));
        //     if (tipHitbox.Intersects(targetHitbox))
        //     {
        //         return true;
        //     }

        //     return false;
        // }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_electroWhipPoints.Count < 2)
                return false;

            // 这里使用你前面想要回归的“原版闪电珠弧色”
            // 如果你项目里调色板名称不是 CultistArcWhip，而是 CultistArc，就改成对应名字即可。
            LightningHelper.DrawLightningAlongPoints(
                Main.spriteBatch,
                _electroWhipPoints,
                Projectile.scale,
                LightningPalettes.CultistArcWhip);

            DrawHandleAndTip();

            return false;
        }

        /// <summary>
        /// 给纯闪电鞭补一个手柄辉光和鞭尖爆点，
        /// 避免整条鞭子只剩线段，没有“武器实体感”。
        /// </summary>
        private void DrawHandleAndTip()
        {
            if (_electroWhipPoints.Count == 0)
                return;

            Texture2D extraTexture = TextureAssets.Extra[ExtrasID.CultistLightingArc].Value;

            Vector2 handle = _electroWhipPoints[0];
            Vector2 tip = _electroWhipPoints[^1];

            Color handleColor = new Color(115, 204, 219, 0) * 0.65f;
            Color tipColor = new Color(255, 255, 255, 0) * 0.85f;

            Vector2 origin = extraTexture.Size() * 0.5f;

            Main.EntitySpriteDraw(
                extraTexture,
                handle - Main.screenPosition,
                null,
                handleColor,
                0f,
                origin,
                0.22f,
                SpriteEffects.None,
                0);

            Main.EntitySpriteDraw(
                extraTexture,
                tip - Main.screenPosition,
                null,
                tipColor,
                0f,
                origin,
                0.30f,
                SpriteEffects.None,
                0);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
            target.AddBuff(BuffID.Electrified, 180);
        }
    }
}