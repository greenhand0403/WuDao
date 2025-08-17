using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Buffs;
using WuDao.Common;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace WuDao.Content.Projectiles.Magic
{
    public class WrathLotusProj : ModProjectile
    {
        // public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.LavaBomb}";
        // 视觉参数（可按审美微调）
        private const float StartScale = 1.15f;  // 出生时缩放
        private const float EndScale = 2.10f;  // 呼吸峰值
        private const int LifeTicks = 120;    // 2 秒存活
        private const int RingDustCount = 20;  // 出生/命中一圈尘粒数量
                                               // 在 Projectile 里做动画：
        private SpriteAnimator _anim = new SpriteAnimator();
        private SpriteGrid _grid;
        private Texture2D _tex;
        public override void SetStaticDefaults()
        {
            // 需要拖尾可在此启用（可选）
            // ProjectileID.Sets.TrailCacheLength[Type] = 6;
            // ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void Load()
        {
            _grid = new SpriteGrid(
                start: new Rectangle(0, 0, 32, 32), // 表内左上角第一帧
                frameSize: new Point(32, 32),
                spacing: Point.Zero,                // 帧之间没有像素空隙就填 0
                across: 6,                          // 每行 6 帧
                down: 2,                            // 2 行
                total: 11                          // 实际总帧数 11
            ); // 如上初始化，或在 OnSpawn 里按贴图实际尺寸/行列计算
            _tex = ModContent.Request<Texture2D>("WuDao/Content/Projectiles/Magic/WrathLotusProj").Value;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32; // 命中箱基准（不需要等于贴图）
            Projectile.height = 32;
            Projectile.scale = 2.5f;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic; // 魔法伤害 

            Projectile.timeLeft = LifeTicks;            // 2 秒存活 
            Projectile.penetrate = -1;                  // 可多次命中不同目标
            Projectile.tileCollide = false;             // 放在玩家与光标处，不受地形阻挡
            Projectile.ignoreWater = true;

            // 同一 NPC 的命中冷却
            Projectile.usesLocalNPCImmunity = true;     // 
            Projectile.localNPCHitCooldown = 12;        // 12 tick ≈ 0.2 秒
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            // 出生时在周围画一圈“灵气”粒子向外扩散
            SpawnAuraDust(Projectile.Center, RingDustCount, startRadius: 12f, speed: 3.2f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            _grid.Draw(_tex, _anim.Frame, Projectile.Center, lightColor,
                   Projectile.rotation, Projectile.scale);
            return false;
        }
        public override void AI()
        {
            _anim.Update(ticksPerFrame: 11, frameCount: _grid.TotalFrames, loop: true);
            Projectile.rotation = Projectile.velocity.ToRotation();

            // 轻微旋转
            // Projectile.rotation += 0.06f * Projectile.direction;

            // 呼吸式缩放：0..1 的进度
            // float t = 1f - (Projectile.timeLeft / (float)LifeTicks);
            // float breath = (float)(0.5 + 0.5 * Math.Sin((t * 2f + Projectile.whoAmI * 0.1f) * MathHelper.TwoPi));
            // Projectile.scale = MathHelper.Lerp(StartScale, EndScale, breath);

            // 周期性淡淡光环（颜色偏青）
            // Lighting.AddLight(Projectile.Center, 0.12f, 0.20f, 0.25f);

            // 每 10 tick 喷一圈较小的外扩尘粒，强化“灵气”感觉
            // if (Projectile.timeLeft % 5 == 0)
                // SpawnAuraDust(Projectile.Center, 10, startRadius: 8f, speed: 2.0f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 命中瞬间再放一圈更亮的尘粒
            SpawnAuraDust(Projectile.Center, RingDustCount, startRadius: 14f, speed: 4.0f, brighter: true);
        }

        public override void OnKill(int timeLeft)
        {
            // 结束时再来一圈、半径更大、速度更慢的散逸
            SpawnAuraDust(Projectile.Center, RingDustCount + 6, startRadius: 18f, speed: 2.2f);
        }

        public override bool? CanCutTiles() => false;

        // 如果你想让“视觉大小”和“命中箱”完全一致，也可在这里动态改伤害判定盒：
        // public override void ModifyDamageHitbox(ref Rectangle hitbox) { ... }

        // 简单使用本体贴图的默认绘制；若你想用精灵图表，可在此自绘 PreDraw
        // public override bool PreDraw(ref Color lightColor) { ... }

        // —— 工具：环形尘粒 —— //
        private static void SpawnAuraDust(Vector2 center, int count, float startRadius, float speed, bool brighter = false)
        {
            // 使用接近幽灵/灵气感的 Dust
            int dustType = DustID.DungeonSpirit;
            for (int i = 0; i < count; i++)
            {
                float ang = MathHelper.TwoPi * i / count;
                Vector2 dir = ang.ToRotationVector2();

                // 随机轻微扰动更自然
                Vector2 pos = center + dir * startRadius + Main.rand.NextVector2Circular(2f, 2f);
                Vector2 vel = dir * speed + Main.rand.NextVector2Circular(0.5f, 0.5f);

                var clr = brighter ? new Color(170, 230, 255) : new Color(140, 200, 255);
                int d = Dust.NewDust(pos, 0, 0, dustType, 0f, 0f, Alpha: 120, clr, Scale: brighter ? 1.2f : 1.0f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = vel;
            }

        }
    }
}