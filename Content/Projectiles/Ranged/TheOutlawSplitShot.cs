using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
namespace WuDao.Content.Projectiles.Ranged
{
    // 机械后解锁的“分裂射弹”：穿透1；命中/撞墙/飞行短时后分裂成左右爆破弹；世纪之花后分裂时必定生成火墙
    public class TheOutlawSplitShot : ModProjectile
    {
        private bool _split;
        private bool _spawnedFirewall;
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("分裂射弹");
        // }
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MiniNukeGrenadeI}";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60; // 飞行很短距离
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // 贴图朝右，需要旋转90°对齐速度方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90);
            Lighting.AddLight(Projectile.Center, 0.25f, 0.2f, 0.1f);
            // ——更明显的轨迹粒子——
            // 每帧 2~4 个，沿速度方向拉出拖尾
            int count = Main.rand.Next(2, 5);
            for (int i = 0; i < count; i++)
            {
                // 在弹体附近随机一点点偏移
                Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);

                // 让粒子沿着当前速度的反方向缓慢回拖（制造尾迹）
                Vector2 dustVel = -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.15f);

                Dust d = Dust.NewDustPerfect(spawnPos, DustID.Smoke, dustVel, 140, default, Main.rand.NextFloat(1.1f, 1.6f));
                d.noGravity = true;                       // 悬浮更明显
                d.fadeIn = Main.rand.NextFloat(1.0f, 1.4f);
            }

            if (Projectile.timeLeft == 30 && !_split)
            {
                DoSplit(); // 飞行到一半自动分裂
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 先做你现有的“分裂成左右小弹”的逻辑（如果还没做过）
            if (!_split) DoSplit(); // 这会生成两颗小分裂弹并Kill自己（如你之前的写法）

            // 仅“砖块/平台”碰撞时考虑火墙生成
            if (NPC.downedPlantBoss) // 世纪之花后才有火墙
            {
                // 判断撞的是水平面还是竖直面：
                // 一般来说，撞地/顶 oldVelocity.Y 的幅度 > oldVelocity.X；撞墙则反之
                bool hitHorizontal = Math.Abs(oldVelocity.Y) >= Math.Abs(oldVelocity.X); // 地板/天花板
                float rotation;

                if (hitHorizontal)
                {
                    // 水平火墙：沿着 X 轴展开，贴图朝上（你的火墙贴图默认朝上）
                    rotation = 0f; // 0度
                }
                else
                {
                    // 垂直火墙：沿着 Y 轴展开，像爬藤怪法杖（贴图向右，旋转90°）
                    rotation = MathHelper.PiOver2; // 90度
                }

                // 生成火墙（只这一处会生成；空中/撞NPC不生成）
                SpawnFirewall(rotation);
            }

            // 让本体自然消亡（或直接 return true）
            return true;
        }
        private void SpawnFirewall(float rotation)
        {
            if (_spawnedFirewall) return;
            if (!NPC.downedPlantBoss) return;              // 世纪之花后才有火墙
            if (Projectile.owner != Main.myPlayer) return; // 避免联机重复生成

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<TheOutlawFirewall>(),
                (int)(Projectile.damage * 0.75f),
                0f,
                Projectile.owner,
                ai0: 120f,        // 2秒
                ai1: rotation     // 0=水平、Pi/2=竖直
            );
            _spawnedFirewall = true;
        }
        public override void OnKill(int timeLeft)
        {
            if (!_split) DoSplit();
            // 小爆点特效
            for (int i = 0; i < 8; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2), 150, default, 1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!_split) DoSplit();
        }

        private void DoSplit()
        {
            if (_split) return; // 双保险
            _split = true;

            if (!Projectile.active) return;

            var src = Projectile.GetSource_FromThis();

            // 以当前飞行方向为基准，顺/逆时针旋转 90°
            // 注意：数学上 +90° 为逆时针（CCW），-90° 为顺时针（CW）
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX); // 速度单位向量
            float speed = 10f;

            Vector2 cw = dir.RotatedBy(-MathHelper.PiOver2) * speed; // 顺时针 90°
            Vector2 ccw = dir.RotatedBy(MathHelper.PiOver2) * speed; // 逆时针 90°

            // 立即爆炸的爆破弹（只飞极短时间）
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(src, Projectile.Center, cw,
                    ModContent.ProjectileType<TheOutlawSplitBomblet>(),
                    (int)(Projectile.damage * 1f), Projectile.knockBack, Projectile.owner);

                Projectile.NewProjectile(src, Projectile.Center, ccw,
                    ModContent.ProjectileType<TheOutlawSplitBomblet>(),
                    (int)(Projectile.damage * 1f), Projectile.knockBack, Projectile.owner);
            }

            // 要求：本体在分裂后消失
            // 若该函数来自 AI/命中/撞墙，直接 Kill 即可；若来自 OnKill，再次调用也安全，因为 _split=true 会阻止重复生成
            if (Projectile.timeLeft > 2)
            {
                Projectile.Kill();
            }
        }

    }
    // 分裂产生的“爆破弹”：极短飞行后立刻爆炸，造成小范围伤害
    public class TheOutlawSplitBomblet : ModProjectile
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("分裂爆破弹");
        // }
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.GrenadeI}";
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10; // 立刻爆
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void OnKill(int timeLeft)
        {
            // 生成“一帧爆炸”投射物，使用更大的碰撞箱来造成伤害
            var src = Projectile.GetSource_FromThis();
            int boom = Projectile.NewProjectile(src, Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<InstantExplosion>(), (int)(Projectile.damage * 1.0f), 0f, Projectile.owner, ai0: 64f);
            // 特效
            for (int i = 0; i < 12; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3), 80, default, 1.2f);
        }
    }

    // “一帧爆炸”投射物，通过扩大宽高覆盖范围来命中
    public class InstantExplosion : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.SnowBallFriendly}";
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2; // 非0即可
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }

        public override void AI()
        {
            // ——更“爆裂”的短程粒子——
            int count = Main.rand.Next(3, 6);
            for (int i = 0; i < count; i++)
            {
                // 小爆弹更亮一些（用火焰/火花色）
                Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(3f, 3f);

                // 速度更散一点，像喷溅
                Vector2 dustVel = -Projectile.velocity * Main.rand.NextFloat(0.08f, 0.18f) + Main.rand.NextVector2Circular(0.5f, 0.5f);

                Dust d = Dust.NewDustPerfect(spawnPos, DustID.Torch, dustVel, 100, default, Main.rand.NextFloat(1.2f, 1.8f));
                d.noGravity = true;
                d.fadeIn = Main.rand.NextFloat(1.1f, 1.5f);
            }

            // ai0 存半径像素
            float radius = Projectile.ai[0] <= 0 ? 64f : Projectile.ai[0];
            // 扩大碰撞箱：以中心为圆近似的矩形覆盖
            Projectile.position -= new Vector2(radius, radius);
            Projectile.width = Projectile.height = (int)(radius * 2f);
            Projectile.Center = Projectile.position + new Vector2(Projectile.width / 2f, Projectile.height / 2f);
            // 一次后重置以避免反复扩大
            Projectile.ai[0] = 0f;
        }
    }
    // 分裂时产生的火墙（世纪之花后解锁）
    public class TheOutlawFirewall : ModProjectile
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("火墙");
        // }
        // 自己管理的本地免疫（命中间隔）
        private const int LocalHitCD = 20; // 每20tick(约0.33s)对同一NPC结算一次
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.SnowBallFriendly}";
        // 取精灵图表里面的火墙
        private SpriteSheet _sheet;
        private SpriteAnimator _anim = new SpriteAnimator();

        // 选择要用的精灵索引（根据你 AddSprite 的顺序）
        // 根据 Common/SpriteSheetsSys.cs 中添加精灵图的顺序
        public int SpriteIndex = 1;
        public override void SetDefaults()
        {
            Projectile.width = 128; // 视觉上是一堵墙，可按需调
            Projectile.height = 16;
            // 不用原生命中判断，自己改了判定
            // Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 120; // 2秒
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = LocalHitCD;
            Projectile.netImportant = true;
            _sheet = SpriteSheets.Get(SpriteAtlasId.RedEffect);
        }

        public override void AI()
        {
            // 根据 SpriteIndex 对应的帧数来更新动画（这里假设大多数是3帧；单帧也 ok）
            int frameCount = _sheet.Sprites[SpriteIndex].FrameCount;
            _anim.Update(ticksPerFrame: 4, frameCount: frameCount, loop: true);

            Projectile.rotation = Projectile.ai[1]; // 0=水平(贴图朝上), Pi/2=竖直

            // 贴图几何：按你视觉来调
            float totalLen = 160f;  // 火墙总长度
            float thickness = 24f;  // 火墙厚度

            bool vertical = Math.Abs(MathHelper.WrapAngle(Projectile.rotation - MathHelper.PiOver2)) < MathHelper.PiOver4;

            Vector2 center = Projectile.Center;
            Vector2 topLeft, size;
            if (!vertical)
            {
                // 水平墙：长度沿 X，厚度沿 Y
                topLeft = new Vector2(center.X - totalLen * 0.5f, center.Y - thickness * 0.5f);
                size = new Vector2(totalLen, thickness);
            }
            else
            {
                // 竖直墙：长度沿 Y，厚度沿 X（宽高互换）
                topLeft = new Vector2(center.X - thickness * 0.5f, center.Y - totalLen * 0.5f);
                size = new Vector2(thickness, totalLen);
            }

            // 免疫递减
            for (int i = 0; i < Main.maxNPCs; i++)
                if (Projectile.localNPCImmunity[i] > 0) Projectile.localNPCImmunity[i]--;

            // 命中
            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC npc = Main.npc[n];
                if (!npc.active || npc.friendly || npc.life <= 0) continue;

                if (Collision.CheckAABBvAABBCollision(npc.position, npc.Size, topLeft, size))
                {
                    if (Projectile.localNPCImmunity[n] <= 0)
                    {
                        var hit = new NPC.HitInfo
                        {
                            Damage = Projectile.damage,
                            Knockback = 0f,
                            HitDirection = (!vertical ? (npc.Center.X >= center.X ? 1 : -1)
                                                      : (npc.Center.Y >= center.Y ? 1 : -1)),
                            Crit = false
                        };
                        npc.StrikeNPC(hit, fromNet: false);
                        Projectile.localNPCImmunity[n] = Projectile.localNPCHitCooldown > 0 ? Projectile.localNPCHitCooldown : 20;
                        Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch);
                    }
                }
            }

            // 你原来的粉尘/发光/自定义绘制保留
            for (int i = 0; i < 3; i++)
            {
                Vector2 p = center + new Vector2(Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2), Main.rand.NextFloat(-4, 4));
                Dust.NewDustPerfect(p, DustID.Torch, Vector2.Zero, 100, default, Main.rand.NextFloat(1.0f, 1.4f));
            }
            Lighting.AddLight(center, 0.9f, 0.45f, 0.1f);
        }
        // 绘制自己的火墙
        public override bool PreDraw(ref Color lightColor)
        {
            _sheet.Draw(SpriteIndex, _anim.Frame, Projectile.Center, lightColor, Projectile.rotation, Projectile.scale * 2);
            return false;
        }
    }
}
