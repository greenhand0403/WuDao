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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4; // 拖尾记录长度（可以调大）
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // 拖尾模式：0 = 线性轨迹
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Shuriken); // 克隆原版行为
            AIType = ProjectileID.Shuriken;
            Projectile.extraUpdates = 1; // 提升更新频率，让拖尾更平滑
        }
        public override void AI()
        {
            // 每帧撒一个尾焰粒子（很少）
            if (Main.rand.NextBool(3)) // 约每 3 帧生成一次
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GoldFlame, // 金色粒子
                    Projectile.velocity.X * 0.1f,
                    Projectile.velocity.Y * 0.1f,
                    100, default, 1.2f
                );
                dust.noGravity = true;
                dust.fadeIn = 0.5f; // 渐隐效果
            }
        }

        public override void PostAI()
        {
            // 在飞行过程中添加微弱发光
            // Lighting.AddLight(Projectile.Center, new Vector3(0.9f, 0.8f, 0.1f) * 0.4f); // 金黄色光，亮度低
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + origin - Main.screenPosition;
                float opacity = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                Color trailColor = new Color(255, 220, 80, 127) * opacity; // 偏金黄色尾焰
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true; // 还绘制主 Projectile
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CreateGoldenDusts(Projectile.position, Projectile.velocity);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position); // 撞击声

        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateGoldenDusts(Projectile.position, oldVelocity);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position); // 撞击声

            return true; // 撞墙后正常销毁
        }

        private void CreateGoldenDusts(Vector2 pos, Vector2 vel)
        {
            for (int i = 0; i < 10; i++) // 数量更多
            {
                Dust dust = Dust.NewDustDirect(pos, Projectile.width, Projectile.height,
                    DustID.GoldFlame,
                    vel.X * 0.3f + Main.rand.NextFloat(-1f, 1f),
                    vel.Y * 0.3f + Main.rand.NextFloat(-1f, 1f),
                    50, default, 1.3f); // 更大、透明度低
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
        }
    }
}
