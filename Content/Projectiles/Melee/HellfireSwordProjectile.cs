using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using WuDao.Common;
using Terraria.ID;

namespace WuDao.Content.Projectiles.Melee
{
    // 地狱之锋的射弹
    public class HellfireSwordProjectile : ModProjectile
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Melee/HellfireSword";
        private SpriteSheet _sheet;
        private SpriteAnimator _anim = new SpriteAnimator();

        // 选择要用的精灵索引（根据你 AddSprite 的顺序）
        public int SpriteIndex = 2;

        public override void SetDefaults()
        {
            Projectile.width = 11;//22
            Projectile.height = 32;//64
            Projectile.friendly = true;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.light = 0.3f;
            _sheet = SpriteSheets.Get(SpriteAtlasId.RedEffect);
            // 精灵图表的使用方法
            // _anim = new SpriteAnimator();

            // _sheet = SpriteSheet
            //     .FromTexture("WuDao/Assets/RedEffect")
            //     // 第1列：若是若干“单帧图标”，就逐个 AddSprite，frameCount=1，frameStep=(0,0)
            //     .AddSprite(new Rectangle(0, 0, 32, 32), new Point(0, 0), 1)   // 图标A
            //     .AddSprite(new Rectangle(0, 32, 32, 32), new Point(0, 0), 1)   // 图标B
            //                                                                    // ...（按你的图标继续加）

            //     // 第2列：假设有一个“高矩形”的横向3帧，从(128, 0)开始，单帧 32x64
            //     .AddSprite(new Rectangle(64, 256, 32, 64), new Point(32, 0), 3); // 横向3帧

            // 第3列：一个正方形 32x32 的横向3帧，从(128, 128)开始
            // .AddSprite(new Rectangle(128, 128, 32, 32), new Point(32, 0), 3)

            // 第4列：如果有纵向3帧，从(256, 0)开始
            // .AddSprite(new Rectangle(256, 0, 32, 32), new Point(0, 32), 3)

            // 第5列：再来一个 48x48 的横向3帧（说明支持不等宽高）
            // .AddSprite(new Rectangle(384, 0, 48, 48), new Point(48, 0), 3);
        }

        public override void AI()
        {
            // 根据 SpriteIndex 对应的帧数来更新动画（这里假设大多数是3帧；单帧也 ok）
            int frameCount = _sheet.Sprites[SpriteIndex].FrameCount;
            _anim.Update(ticksPerFrame: 3, frameCount: frameCount, loop: true);

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _sheet.Draw(SpriteIndex, _anim.Frame, Projectile.Center, lightColor, Projectile.rotation, Projectile.scale * 2);
            return false;
        }
        // HellfireSwordProjectile.cs （剑气命中）
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.25f)
                target.AddBuff(BuffID.OnFire, 120);
        }
        // HellfireSwordProjectile.cs
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 拿到发射者玩家并判定是否在地狱层
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
            {
                Player owner = Main.player[Projectile.owner];
                if (owner?.active == true && owner.ZoneUnderworldHeight)
                {
                    modifiers.FinalDamage *= 1.20f;
                }
            }
        }
    }
}