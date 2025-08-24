using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
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
        // 字段
        private int _deferSplitTicks = 0;
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("分裂射弹");
        // }
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MiniNukeGrenadeI}";
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240; // 飞行很短距离
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.MaxUpdates = 2;
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

            // ——— 分裂时机（避免先分裂错过 Tile 碰撞） ———
            if (!_split)
            {
                if (Projectile.timeLeft <= 40) // 进入“可能分裂”的窗口
                {
                    if (IsImminentTileCollision())
                    {
                        _deferSplitTicks = Math.Max(_deferSplitTicks, 3); // 等 3 帧让 OnTileCollide 先发生
                    }
                    else if (Projectile.timeLeft <= 32)
                    {
                        if (_deferSplitTicks > 0) _deferSplitTicks--; // 继续等待
                        else DoSplit(); // 真不靠近地形了，才空中分裂
                    }
                }
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player p = Main.player[Projectile.owner];
            if (p != null && p.active)
            {
                Vector2 dir = Projectile.velocity;
                if (dir.LengthSquared() < 0.001f) dir = Vector2.UnitX; else dir.Normalize();

                Projectile.Center = p.Center + dir * 8f; // 从玩家中心稍微向射线方向偏 6px
                Projectile.netUpdate = true;
            }
        }
        // 预测“下一小段路径上是否会撞地形”（扫掠 3 步，每步 8px）
        private bool IsImminentTileCollision()
        {
            Vector2 dir = Projectile.velocity;
            if (dir.LengthSquared() < 0.001f) return false;
            dir.Normalize();
            Vector2 step = dir * 8f;

            for (int i = 1; i <= 3; i++)
            {
                Vector2 checkPos = Projectile.position + step * i;
                if (Collision.SolidCollision(checkPos, Projectile.width, Projectile.height))
                    return true;
            }
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!_split) DoSplit();   // 只做分裂；是否生成火墙由 DoSplit() 内部决定
            return true;              // Kill；OnKill 不再生成火墙
        }
        // 分裂瞬间：探测附近是否有可贴靠的实心（地/左/右），命中则生成火墙
        private void TrySpawnFirewallAtSplit()
        {
            if (_spawnedFirewall || !NPC.downedPlantBoss || Projectile.owner != Main.myPlayer) return;

            // 取样偏移：沿法向探 10/16px，覆盖高速或边缘缝
            const float probeNear = 10f, probeFar = 16f;

            // 地面（只要下方是实心砖/斜坡/半砖，平台忽略）
            if (IsStrictSolidBlockAtWorld(Projectile.Center + new Vector2(0f, +probeNear)) ||
                IsStrictSolidBlockAtWorld(Projectile.Center + new Vector2(0f, +probeFar)))
            {
                SpawnFirewall(0); // 贴地水平
                return;
            }

            // 左墙
            if (IsStrictSolidBlockAtWorld(Projectile.Center + new Vector2(-probeNear, 0f)) ||
                IsStrictSolidBlockAtWorld(Projectile.Center + new Vector2(-probeFar, 0f)))
            {
                SpawnFirewall(+1); // 贴左墙（贴图朝右）
                return;
            }

            // 右墙
            if (IsStrictSolidBlockAtWorld(Projectile.Center + new Vector2(+probeNear, 0f)) ||
                IsStrictSolidBlockAtWorld(Projectile.Center + new Vector2(+probeFar, 0f)))
            {
                SpawnFirewall(-1); // 贴右墙（贴图朝左）
                return;
            }

            // 天花板或周围都不是实心 → 不生成（满足“只在实心物块才生成”的规则）
        }

        // 严格实心检测：未激活、参与碰撞的砖/斜坡/半砖；平台排除
        private bool IsStrictSolidBlockAtWorld(Vector2 worldPos)
        {
            int tx = (int)(worldPos.X / 16f);
            int ty = (int)(worldPos.Y / 16f);
            if (!WorldGen.InWorld(tx, ty, 10)) return false;

            Tile t = Framing.GetTileSafely(tx, ty);
            if (!t.HasUnactuatedTile) return false;                   // 激活砖不碰撞
            if (TileID.Sets.Platforms[t.TileType]) return false;      // 平台排除
            if (WorldGen.SolidOrSlopedTile(tx, ty)) return true;      // 普通/斜坡/半砖
            if (Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType]) return true; // 兜底
            return false;
        }

        // 原先只有 orient，现在加一个可选 overrideCenter
        private void SpawnFirewall(int orient, Vector2? overrideCenter = null)
        {
            if (_spawnedFirewall) return;
            if (!NPC.downedPlantBoss) return;
            if (Projectile.owner != Main.myPlayer) return;

            Vector2 pos = overrideCenter ?? Projectile.Center; // ← 支持指定中心
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                pos,                        // ★ 用这个位置
                Vector2.Zero,
                ModContent.ProjectileType<TheOutlawFirewall>(),
                (int)(Projectile.damage * 0.75f),
                0f,
                Projectile.owner,
                ai0: 120f,
                ai1: orient                 // -1右墙/0地面/+1左墙（你之前的方位枚举）
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

            // ★ 命中后补墙：未生成 → 优先贴地，其次至少放在“身体中心”
            if (!_spawnedFirewall && NPC.downedPlantBoss && Projectile.owner == Main.myPlayer)
            {
                // 1) 三列X位（左1/4、中、右1/4），向下扫描若干格，找“最近的实心顶面”
                Vector2 footMid = new Vector2(target.Center.X, target.Bottom.Y);
                float quarter = target.width * 0.25f;
                float[] xs = new float[] { footMid.X - quarter, footMid.X, footMid.X + quarter };

                // 最多向下找几格地（按需要调，8~12格较稳）
                const int maxTilesDown = 12;

                if (TryFindSolidTopUnder(xs, footMid.Y, maxTilesDown, out Vector2 groundPos))
                {
                    // 把火墙中心放到这块砖的顶面（火墙AI里会再沿法向上抬你设定的像素，比如10~16px）
                    SpawnFirewall(0, groundPos + new Vector2(0, -4));
                    return;
                }

                // 2) 脚下不是实心 → 至少“生成在它的中心”
                //    避免生成到头顶：直接用 Center
                SpawnFirewall(0, target.Center);
            }
        }
        // 尝试在给定的 X 列（数组）中，从 startY 往下最多 maxTilesDown 格，找到第一个“严格实心”的顶面；找到则返回那个世界坐标
        private bool TryFindSolidTopUnder(float[] xs, float startY, int maxTilesDown, out Vector2 pos)
        {
            // 从最靠近的那一列往下找，谁先找到用谁
            for (int xi = 0; xi < xs.Length; xi++)
            {
                int startTy = (int)(startY / 16f);
                int tx = (int)(xs[xi] / 16f);

                for (int ty = startTy; ty <= startTy + maxTilesDown; ty++)
                {
                    if (!WorldGen.InWorld(tx, ty, 10)) break;

                    Tile t = Framing.GetTileSafely(tx, ty);
                    // 必须未激活 + 不是平台 + 实心/斜坡/半砖
                    if (t.HasUnactuatedTile &&
                        !TileID.Sets.Platforms[t.TileType] &&
                        (WorldGen.SolidOrSlopedTile(tx, ty) || (Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType])))
                    {
                        float topY = ty * 16f;                  // 砖块顶面（世界坐标）
                        pos = new Vector2(xs[xi], topY);        // 以该列X + 顶面Y 作为火墙中心的基准
                        return true;
                    }
                }
            }

            pos = default;
            return false;
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

            // ★ 分裂瞬间：仅当贴靠到实心物块时才生成火墙
            TrySpawnFirewallAtSplit();
            Projectile.Kill();
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
            Projectile.timeLeft = 20; // 立刻爆
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
                                           // 用于命中/绘制的一致几何（你可以按贴图像素改）
        public const float TotalLen = 120f; // 墙“长度”
        public const float Thickness = 28f;  // 墙“厚度”
        public const int LifeTicks = 120;
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.SnowBallFriendly}";
        // 读取方位（ai1存的 int：-1 / 0 / +1）
        private int Orientation => (int)Projectile.ai[1]; // -1右墙,0水平,+1左墙
        private bool IsVertical => Orientation != 0;
        // 选择要用的精灵索引（根据你 AddSprite 的顺序）
        // 根据 Common/SpriteSheetsSys.cs 中添加精灵图的顺序
        // public int SpriteIndex = 1;
        // 取精灵图表里面的火墙
        // private SpriteSheet _sheet;
        // 行优先精灵网格
        private SpriteGrid _grid;
        private Texture2D _tex;
        private SpriteAnimator _anim = new SpriteAnimator();
        public override void SetDefaults()
        {
            Projectile.width = (int)TotalLen; // 视觉上是一堵墙，可按需调
            Projectile.height = (int)Thickness;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = LifeTicks; // 2秒
            Projectile.usesLocalNPCImmunity = true;
            Projectile.netImportant = true;
            Projectile.localNPCHitCooldown = LocalHitCD;
            Projectile.light = 0.3f;

            // _sheet = SpriteSheets.Get(SpriteAtlasId.RedEffect);
            // 加载问题，放到load里面无法加载
            // 如上初始化，或在 OnSpawn 里按贴图实际尺寸/行列计算
            _grid = new SpriteGrid(
                start: new Rectangle(0, 0, 32, 32), // 表内左上角第一帧
                frameSize: new Point(32, 32),
                spacing: Point.Zero,                // 帧之间没有像素空隙就填 0
                across: 6,                          // 每行 6 帧
                down: 2,                            // 2 行
                total: 11                          // 实际总帧数 11
            );
            _tex = ModContent.Request<Texture2D>("WuDao/Content/Projectiles/Ranged/TheOutlawFirewall").Value;
        }

        public override void AI()
        {
            // 根据 SpriteIndex 对应的帧数来更新动画（这里假设大多数是3帧；单帧也 ok）
            // int frameCount = _sheet.Sprites[SpriteIndex].FrameCount;
            // _anim.Update(ticksPerFrame: 4, frameCount: frameCount, loop: true);
            _anim.Update(ticksPerFrame: LifeTicks / 2 / _grid.TotalFrames, frameCount: _grid.TotalFrames, loop: true);
            // 仅用于“贴图方向”的rotation
            Projectile.rotation = Orientation switch
            {
                0 => 0f,
                1 => MathHelper.PiOver2,   // 左墙，贴图朝右
                -1 => -MathHelper.PiOver2,  // 右墙，贴图朝左
                _ => 0f
            };
            // 射弹中心位置改变
            if (Projectile.localAI[0] == 0f)
            {
                Vector2 snap = Vector2.Zero;
                const float floorLift = 0f; // 地面抬高
                const float wallPush = 5f; // 贴墙外推 10px（左右一致）

                if (Orientation == 0) snap = new Vector2(0f, -floorLift); // 贴地向上抬
                else if (Orientation == 1) snap = new Vector2(+wallPush, 0f);  // 贴左墙→向右推
                else if (Orientation == -1) snap = new Vector2(-wallPush, 0f);  // 贴右墙→向左推

                if (snap != Vector2.Zero)
                {
                    Projectile.Center += snap;
                    Projectile.netUpdate = true;
                }
                Projectile.localAI[0] = 1f; // 打上“已偏移”标记
            }

            Vector2 center = Projectile.Center;
            Lighting.AddLight(center, 0.9f, 0.45f, 0.1f);

            // 记录中心 → 改尺寸 → 还原中心（否则会跳位）
            int w = IsVertical ? (int)Thickness : (int)TotalLen;
            int h = IsVertical ? (int)TotalLen : (int)Thickness;
            if (Projectile.width != w || Projectile.height != h)
            {
                Projectile.width = w;
                Projectile.height = h;
                Projectile.Center = center;      // 还原中心
                Projectile.netUpdate = true;
            }
        }

        // 绘制自己的火墙 水平方向也有偏移 例如打到左侧墙壁上，贴图应该往右边移动20像素
        public override bool PreDraw(ref Color lightColor)
        {
            // 朝向判断（能识别 +90° / -90°）
            float rot = MathHelper.WrapAngle(Projectile.rotation);
            bool vertical = Math.Abs(Math.Abs(rot) - MathHelper.PiOver2) < MathHelper.PiOver4;
            Rectangle rec = _grid.GetFrameRect(1);
            // ★ 仅在“水平”时抬高贴图 10px（命中盒子不动）
            Vector2 drawCenter = Projectile.Center;
            float halfExtentAlongNormal = rec.Width / 2; // 未缩放时 “半边” 长度（像素）
            // 贴图偏移
            const float scale = 4f;
            Vector2 snap = Vector2.Zero;

            if (Orientation == 0)
            {
                snap = new Vector2(0f, -1);// 贴地向上抬
            }
            else if (Orientation == 1)
            {
                snap = new Vector2(+1, 0f);// 贴左墙→向右推
            }
            else if (Orientation == -1)
            {
                snap = new Vector2(-1, 0f);// 贴右墙→向左推
            }

            if (snap != Vector2.Zero)
            {
                drawCenter += snap * ((scale - 1f) * halfExtentAlongNormal) + 4 * snap;//额外抬4像素
                Projectile.netUpdate = true;
            }

            // _sheet.Draw(SpriteIndex, _anim.Frame, drawCenter, lightColor, rot, Projectile.scale * 2);
            // _grid.Draw(_tex, _anim.Frame, drawCenter, lightColor,
            //    rot, Projectile.scale * 4);
            _grid.Draw(_tex, _anim.Frame, drawCenter, lightColor, rot, scale);
            return false;
        }
        // ★★ 关键：用 Colliding 覆盖默认碰撞，按朝向给出“水平/竖直”的 AABB
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 c = Projectile.Center;
            Vector2 topLeft, size;

            if (!IsVertical)
            {
                topLeft = new Vector2(c.X - TotalLen * 0.5f, c.Y - Thickness * 0.5f);
                size = new Vector2(TotalLen, Thickness);
            }
            else
            {
                topLeft = new Vector2(c.X - Thickness * 0.5f, c.Y - TotalLen * 0.5f);
                size = new Vector2(Thickness, TotalLen);
            }

            bool hit = Collision.CheckAABBvAABBCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                topLeft, size
            );
            return hit;
        }
    }
}
