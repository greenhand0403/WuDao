using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using WuDao.Common;
using Terraria.ID;

namespace WuDao.Content.Projectiles.Melee
{
    public class HellfireSwordProjectile : ModProjectile
    {
        // 仅占位用，此处贴图路径不起作用，后续自己绘制贴图
        public override string Texture => "WuDao/Content/Items/Weapons/Melee/HellfireSword";
        private SpriteSheet _sheet;
        private SpriteAnimator _anim = new SpriteAnimator();

        // 选择要用的精灵索引（根据你 AddSprite 的顺序）
        // 根据 Common/SpriteSheetsSys.cs 中添加精灵图的顺序
        public int SpriteIndex = 0;

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.light = 0.3f;
            _sheet = SpriteSheets.Get(SpriteAtlasId.RedEffect);
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