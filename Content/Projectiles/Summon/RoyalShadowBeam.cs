using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Summon
{
    // 模仿暗影束法杖：持续极短时间的“直线射线”，穿墙、不弹射、可多段命中
    public class RoyalShadowBeam : ModProjectile
    {
        const float MaxBeamLength = 1200f; // 最大射线长度
        const float BeamWidth = 12f;       // 打击判定宽度（像素）
        const int BeamLife = 12;           // 存活帧数（越短越像“脉冲束”）
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
        // 彩虹 Dust 颜色
        static readonly Color[] Rainbow =
        {
            Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Magenta
        };

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("皇家暗影束");
        }

        public override void SetDefaults()
        {
            Projectile.width = (int)BeamWidth;
            Projectile.height = (int)BeamWidth;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;      // 穿墙
            Projectile.ignoreWater = true;
            Projectile.timeLeft = BeamLife;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10; // 快速多段但有冷却
            Projectile.hide = true;              // 自绘（只画束，不画本体）
        }

        // ai[0] = 发射器（头顶权杖）的 whoAmI
        // velocity 的方向向量：在生成时由 minion 填充（速度大小不重要）
        public override void OnSpawn(IEntitySource source)
        {
            // 方向单位化（保留你原有的逻辑）
            if (Projectile.velocity.LengthSquared() < 0.001f)
                Projectile.velocity = Vector2.UnitY;
            else
                Projectile.velocity.Normalize();

            // 起点 = 发射器中心（头顶权杖）
            Vector2 start = Main.projectile[(int)Projectile.ai[0]].Top - Vector2.UnitY * 4;

            // 计算一次长度（对最近目标测距；没有目标就用上限）
            float length = MaxBeamLength;
            int target = FindClosestNPC(start, MaxBeamLength);
            if (target != -1)
            {
                float d = Vector2.Distance(start, Main.npc[target].Center);
                length = MathHelper.Clamp(d, 16f, MaxBeamLength);
            }

            // 只在生成的这一刻喷一次彩虹尘
            SpawnRainbowDust(start, Projectile.velocity, length, 48);

            // 存起来给 PreDraw/命中判定用（可复用；AI里也能继续更新命中判定）
            Projectile.localAI[0] = length;
        }

        public override void AI()
        {
            // 保持起点跟随权杖
            Projectile.Center = Main.projectile[(int)Projectile.ai[0]].Center;

            float length = Projectile.localAI[0] > 0 ? Projectile.localAI[0] : MaxBeamLength;

            // 击中检测仍按这段长度
            HitScanNPCs(Projectile.Center, Projectile.velocity, length);
        }
        int FindClosestNPC(Vector2 from, float range)
        {
            int target = -1;
            float best = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.CanBeChasedBy())
                {
                    float d = Vector2.Distance(from, n.Center);
                    if (d < best)
                    {
                        best = d;
                        target = i;
                    }
                }
            }
            return target;
        }

        void HitScanNPCs(Vector2 start, Vector2 dir, float length)
        {
            Rectangle lineRect = new Rectangle(0, 0, (int)length, (int)BeamWidth);
            // 射线方向上的步进采样：为了简单可靠，对附近 NPC 做线-圆距离检测
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || n.friendly || n.dontTakeDamage || !Projectile.CanHitWithOwnBody(n)) continue;

                // 最近点到线段距离
                float dist = DistancePointToLineSegment(n.Center, start, start + dir * length);
                if (dist <= n.width * 0.5f + BeamWidth * 0.5f)
                {
                    var hitInfo = new NPC.HitInfo
                    {
                        Damage = Projectile.damage,
                        Knockback = 0f,
                        HitDirection = Projectile.direction,
                        Crit = false
                    };
                    n.StrikeNPC(hitInfo, fromNet: false);
                    // 使用本地免疫：不会瞬间打爆
                    n.immune[Projectile.owner] = Projectile.localNPCHitCooldown;
                }
            }
        }

        static float DistancePointToLineSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ap = p - a;
            Vector2 ab = b - a;
            float ab2 = Vector2.Dot(ab, ab);
            float t = ab2 == 0 ? 0 : MathHelper.Clamp(Vector2.Dot(ap, ab) / ab2, 0f, 1f);
            Vector2 closest = a + ab * t;
            return Vector2.Distance(p, closest);
        }

        void SpawnRainbowDust(Vector2 start, Vector2 dir, float length, int points)
        {
            for (int i = 0; i < points; i++)
            {
                float t = i / (points - 1f);
                Vector2 pos = start + dir * (t * length) + Main.rand.NextVector2Circular(0.6f, 0.6f); // ← 更小随机
                int d = Dust.NewDustPerfect(pos, DustID.RainbowTorch, Vector2.Zero, 0, Color.White, 1.0f).dustIndex;
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.05f; // 更稳
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            Vector2 start = Main.projectile[(int)Projectile.ai[0]].Center - Main.screenPosition;
            float length = Projectile.localAI[0] > 0 ? Projectile.localAI[0] : MaxBeamLength; // ← 用AI算的长度
            float rot = Projectile.velocity.ToRotation();

            for (int layer = 0; layer < Rainbow.Length; layer++)
            {
                float w = BeamWidth * (1.2f - layer * 0.12f);
                Color c = Rainbow[layer] * 0.85f;
                Vector2 scale = new Vector2(length, w);
                Main.spriteBatch.Draw(tex, start, null, c, rot, new Vector2(0f, 0.5f), scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
