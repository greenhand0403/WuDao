using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;          // 你前面做的 SpriteSheetsSystem / SpriteSheets
using Microsoft.Xna.Framework.Graphics;

namespace WuDao.Content.Projectiles.Melee
{
    public class GhostChakramProj : ModProjectile
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Melee/GhostChakram";
        // —— 图集索引：BlueEffect 的 index=0（单帧图标）
        private const int BLUE_INDEX = 0;

        // —— 出/回归参数（近似“光辉飞盘”的感觉）
        private const int OutboundTicks = 24;        // 向前飞多少帧后开始回归
        private const float MaxRange = 520f;         // 超过这个距离也回归
        private const float OutSpeed = 14f;
        private const float ReturnSpeed = 17f;
        private const float Steering = 0.15f;        // 回归转向平滑

        // —— 穿透与成长
        private const int MaxPierce = 3;             // 最多穿透 3 敌人
        private const float GrowPerPierce = 0.25f;   // 穿透伤害每次 +25%
        private int pierced;                         // 已穿透次数
        private float sizeScale = 1f;                // 碰撞/绘制缩放同步增长
        private float drawBaseScale = 1.25f;          // 以图标原始尺寸绘制的基准缩放（按需要微调）

        // —— 视觉：淡入
        private int fadeTimer = 8;                   // 前 8 tick 淡入
        private const int BaseHitbox = 40;           // 初始碰撞箱基准（像素，按你的美术留白调节）

        // —— 动画（虽然是单帧，这里保留 Animator 便于将来切换多帧）
        private readonly SpriteAnimator _anim = new SpriteAnimator();
        private SpriteSheet _sheet;
        public override void SetStaticDefaults()
        {
            // 拖尾缓存长度与模式
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // 2=按 oldPos 画平滑拖尾
        }

        public override void SetDefaults()
        {
            Projectile.width = BaseHitbox;
            Projectile.height = BaseHitbox;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;

            Projectile.penetrate = MaxPierce; // 由 tML 负责递减，<=0 就死亡
            Projectile.timeLeft = 600;

            Projectile.tileCollide = false;   // —— 穿墙
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true; // 提高多段命中稳定性
            Projectile.localNPCHitCooldown = 10;

            // 初始透明（淡入）
            Projectile.alpha = 255;
            _sheet = SpriteSheets.Get(SpriteAtlasId.BlueEffect);
            // 单帧：让 Animator 走个形式；若改成多帧，直接改 frameCount 即可
        }
        private bool IsEmpowered => Projectile.ai[1] == 1f;

        public override void OnSpawn(IEntitySource source)
        {
            // 统一初速
            Projectile.velocity = Vector2.Normalize(Projectile.velocity) * OutSpeed;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // —— 朝飞行方向旋转（若贴图朝上，可 +MathHelper.PiOver2）
            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.rotation = Projectile.velocity.ToRotation();

            // —— 淡入
            if (fadeTimer > 0)
            {
                fadeTimer--;
                Projectile.alpha = (int)MathHelper.Clamp(Projectile.alpha - 255f / 8f, 0f, 255f);
            }

            // —— 计时：到点或离太远，就回归
            Projectile.ai[0]++;
            bool shouldReturn =
                Projectile.ai[0] >= OutboundTicks ||
                Vector2.Distance(owner.Center, Projectile.Center) > MaxRange;

            if (shouldReturn)
            {
                // 回归主人：速度朝向玩家平滑插值
                Vector2 toOwner = owner.Center - Projectile.Center;
                float dist = toOwner.Length();
                if (dist < 24f)
                {
                    // 回到玩家身边就消失（模拟飞盘接住的感觉）
                    Projectile.Kill();
                    return;
                }

                toOwner.Normalize();
                Vector2 desired = toOwner * ReturnSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, Steering);

                // 回归期也继续朝前旋转
            }

            // 轻微发光（可选）
            Lighting.AddLight(Projectile.Center, 0.15f, 0.25f, 0.35f);

            if (IsEmpowered)
            {
                // 强化时多加一点光
                Lighting.AddLight(Projectile.Center, 0.25f, 0.35f, 0.55f);

                // 偶尔撒点淡蓝尘（灵气感）
                if (Main.rand.NextBool(3))
                {
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.DungeonSpirit,
                        0f, 0f, 150, new Color(160, 220, 255), 1.1f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity = Projectile.velocity.RotatedByRandom(0.35) * 0.1f;
                }
            }
        }

        // —— 命中：伤害&碰撞箱成长 + 环形灵气粒子
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 生成一圈灵气 Dust
            SpawnAuraDust(Projectile.Center, 16, 2.2f, 6f);

            // 记录穿透次数（tML 自己会扣 penetrate，但我们要根据“已穿透”做成长）
            int hitsSoFar = ++pierced;

            // 下一次命中更痛 + 更大碰撞箱
            if (hitsSoFar <= MaxPierce)
            {
                sizeScale *= (1f + GrowPerPierce);
                Projectile.damage = (int)Math.Ceiling(Projectile.damage * (1f + GrowPerPierce));
                Projectile.netUpdate = true;
            }
        }

        // —— 缩小/放大“伤害判定命中箱”
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            int s = (int)(BaseHitbox * sizeScale);
            Point c = hitbox.Center;
            hitbox.Width = s;
            hitbox.Height = s;
            hitbox.X = c.X - s / 2;
            hitbox.Y = c.Y - s / 2;
        }

        // —— （可选）如果你哪天想开启 tileCollide，可在这里单独给地形碰撞箱做得更宽容
        // public override void TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        // {
        //     int s = (int)(BaseHitbox * sizeScale);
        //     width = height = s;
        // }

        public override bool PreDraw(ref Color lightColor)
        {
            _anim.Update(9999, 1, true);
            Rectangle src = _sheet.GetFrameRect(BLUE_INDEX, frameIndex: 0);
            Vector2 origin = new Vector2(src.Width / 2f, src.Height / 2f);
            // === 1) 拖尾（按 oldPos 由近及远，透明度递减） ===
            int trailLen = ProjectileID.Sets.TrailCacheLength[Projectile.type];
            for (int i = trailLen - 1; i >= 1; i--)
            {
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f;
                float t = i / (float)trailLen; // 0..1
                float alpha = MathHelper.Lerp(0.0f, 0.6f, 1f - t); // 越靠近本体越亮
                float scale = drawBaseScale * sizeScale * MathHelper.Lerp(0.7f, 1.0f, 1f - t);

                // 强化时拖尾更偏蓝、更亮一点
                Color c = IsEmpowered
                    ? new Color(140, 200, 255) * alpha
                    : lightColor * (alpha * 0.8f);

                Main.EntitySpriteDraw(_sheet.Texture,
                    pos - Main.screenPosition,
                    src,
                    c,
                    Projectile.rotation,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0);
            }

            // === 2) 脉冲光圈（只对强化飞盘） ===
            if (IsEmpowered)
            {
                // 用时间驱动的呼吸 + 叠加两圈白/蓝外光
                float t = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.5f + 0.5f; // 0..1
                float baseScale = drawBaseScale * sizeScale;
                float pulse1Scale = baseScale * MathHelper.Lerp(1.12f, 1.28f, t);
                float pulse2Scale = baseScale * MathHelper.Lerp(1.28f, 1.44f, t);

                // 白色内圈（淡）
                Main.EntitySpriteDraw(_sheet.Texture,
                    Projectile.Center - Main.screenPosition,
                    src,
                    new Color(255, 255, 255) * 0.35f,
                    Projectile.rotation,
                    origin,
                    pulse1Scale,
                    SpriteEffects.None, 0);

                // 蓝色外圈（更淡、更大）
                Main.EntitySpriteDraw(_sheet.Texture,
                    Projectile.Center - Main.screenPosition,
                    src,
                    new Color(120, 180, 255) * 0.30f,
                    Projectile.rotation,
                    origin,
                    pulse2Scale,
                    SpriteEffects.None, 0);
            }

            float drawScale = drawBaseScale * sizeScale;
            Color bodyColor = lightColor * (1f - Projectile.alpha / 255f);
            if (IsEmpowered)
                bodyColor = Color.Lerp(bodyColor, new Color(180, 230, 255), 0.25f); // 略偏亮偏蓝
            _sheet.Draw(BLUE_INDEX, _anim.Frame, Projectile.Center, bodyColor, Projectile.rotation, drawScale);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // 结束也放一圈更大的灵气
            // SpawnAuraDust(Projectile.Center, 22, 2.6f, 12f);
        }

        private static void SpawnAuraDust(Vector2 center, int count, float startRadius, float speed)
        {
            // 选择带灵气感的 Dust（按需换颜色/类型）
            int dustType = DustID.DungeonSpirit; // 接近幽灵感
            for (int i = 0; i < count; i++)
            {
                float ang = MathHelper.TwoPi * i / count;
                Vector2 dir = ang.ToRotationVector2();
                Vector2 pos = center + dir * startRadius;

                var d = Dust.NewDustPerfect(pos, dustType, dir * speed, Alpha: 120, new Color(120, 200, 255), Scale: 1.1f);
                d.noGravity = true;
            }
        }
    }
}
