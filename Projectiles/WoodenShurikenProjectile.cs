using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace WuDao.Projectiles
{
    public class WoodenShurikenProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Shuriken); // 克隆原版行为
            AIType = ProjectileID.Shuriken;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // 让颜色变为偏黄色（也可根据光照动态调节）
            return new Color(255, 220, 100, 128);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CreateWoodDusts(Projectile.position, Projectile.velocity);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position); // 撞击声

        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateWoodDusts(Projectile.position, oldVelocity);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position); // 撞击声

            return true; // 撞墙后正常销毁
        }

        private void CreateWoodDusts(Vector2 pos, Vector2 vel)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustDirect(pos, Projectile.width, Projectile.height,
                    DustID.WoodFurniture, // 木屑 Dust
                    vel.X * 0.3f + Main.rand.NextFloat(-1, 1),
                    vel.Y * 0.3f + Main.rand.NextFloat(-1, 1),
                    100, default, 1.2f
                ).noGravity = true;
            }
        }
    }
}
