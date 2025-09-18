using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ID;
using System;

namespace WuDao.Content.Projectiles.Melee
{
    // 负责按节奏陆续生成“马”射弹
    public class StampedeSpawnerProj : ModProjectile
    {
        // —— 配置参数 —— //
        private Rectangle _screenRect;  // 屏幕矩形（世界坐标）
        private int _startX, _targetX;  // 左/右侧出生与目标 X
        private int _count;             // 总匹数（10~15）
        private int _spawned;           // 已生成
        private int _interval;          // 当前批的间隔倒计时
        private int _damage;
        private float _knockback;

        // 垂直边距和抖动系数（按需微调）
        private const int TopPad = 80;
        private const int BottomPad = 80;

        // 每批之间的生成间隔范围（单位：tick）
        private const int IntervalMin = 4;
        private const int IntervalMax = 9;
        private bool _fromLeft; // true = 从左边进，false = 右边进
        public override string Texture => "Terraria/Images/Projectile_0"; // 不绘制自身

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60 * 10; // 最多存活10秒，足够把一波马发完
            Projectile.hide = true;
        }

        public void Setup(Rectangle screenRect, int startX, int targetX, int count, int damage, float knockback)
        {
            _fromLeft = startX < screenRect.Left; // 由第一次的 startX 判定方向
            _count = count;
            _damage = damage;
            _knockback = knockback;
            _spawned = 0;
            _interval = 1; // 立刻生成第一匹
        }
        public override void AI()
        {
            if (Projectile.owner != Main.myPlayer) return;
            if (_spawned >= _count) { Projectile.Kill(); return; }

            if (--_interval <= 0)
            {
                // —— 每次都用“当前屏幕”的世界矩形 —— //
                Rectangle rect = new Rectangle(
                    (int)Main.screenPosition.X,
                    (int)Main.screenPosition.Y - Main.screenHeight,
                    Main.screenWidth,
                    Main.screenHeight
                );

                // 左右边也跟随当前屏幕（可继续用你已有的 _startX/_targetX；建议改为实时）
                int startX = (_startX < rect.Left) ? rect.Left - 100 : rect.Right + 100;
                int targetX = (_startX < rect.Left) ? rect.Right + 320 : rect.Left - 320;

                // 用“百分比边距”适配各种分辨率（避免固定 80px 导致高分屏过窄）
                int top = rect.Top + (int)(rect.Height * 0.12f);
                int bottom = rect.Bottom - (int)(rect.Height * 0.12f);

                // 用 Lerp 按序号均匀铺开（不吃整除误差）
                float t = (_count > 1) ? _spawned / (float)(_count - 1) : 0f;
                float baseY = MathHelper.Lerp(top, bottom, t);

                float y = MathHelper.Clamp(baseY, top + 12, bottom - 12);

                float yTarget = y + Main.rand.Next(-40, 40);

                Vector2 spawn = new Vector2(startX, y);
                Vector2 velDir = (new Vector2(targetX, yTarget) - spawn).SafeNormalize(Vector2.UnitX);
                float speed = Main.rand.NextFloat(10f, 14f);
                Vector2 vel = velDir * speed;

                int proj = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), spawn, vel,
                    ModContent.ProjectileType<HorseItemVariantProjectile>(),
                    _damage, _knockback, Projectile.owner);

                if (proj >= 0)
                {
                    var p = Main.projectile[proj];
                    p.friendly = true;
                    p.hostile = false;
                    p.tileCollide = false;  // 让整队马稳定穿屏
                    p.penetrate = -1;
                    p.timeLeft = 180;
                    p.netUpdate = true;
                }

                _spawned++;
                _interval = Main.rand.Next(4, 9);
            }
        }

    }
}
