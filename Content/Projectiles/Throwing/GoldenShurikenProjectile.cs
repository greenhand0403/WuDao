using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace WuDao.Content.Projectiles.Throwing
{
    public class GoldenShurikenProjectile : ModProjectile
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Throwing/GoldenShuriken";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // 拖尾记录长度（可以调大）
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // 拖尾模式：0 = 线性轨迹
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Shuriken); // 克隆原版行为
            AIType = ProjectileID.Shuriken;
            Projectile.extraUpdates = 1; // 提升更新频率，让拖尾更平滑
            Projectile.alpha = 255; // 淡入效果
            Projectile.timeLeft = 300;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Vector2 drawPos = Projectile.oldPos[i] + origin - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                float opacity = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                Color trailColor = new Color(255, 220, 80, 255) * opacity;

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }
            // 就需要自己手动把本体画出来
            // Vector2 curPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            // Main.EntitySpriteDraw(texture, curPos, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            // return false;
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                CreateGoldenDusts(Projectile.position, Projectile.velocity);
            }
            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.Midas, 120);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                CreateGoldenDusts(Projectile.position, Projectile.velocity);
            }
            return true;
        }

        private void CreateGoldenDusts(Vector2 pos, Vector2 vel)
        {
            Dust.NewDustDirect(pos, Projectile.width, Projectile.height,
                DustID.GoldFlame,
                vel.X * 0.1f,
                vel.Y * 0.1f,
                100, default, 0.85f);
        }
    }
}
