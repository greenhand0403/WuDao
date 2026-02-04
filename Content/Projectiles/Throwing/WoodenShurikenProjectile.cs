using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace WuDao.Content.Projectiles.Throwing
{
    // 木飞镖
    public class WoodenShurikenProjectile : ModProjectile
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Throwing/WoodenShuriken";
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Shuriken); // 克隆原版行为
            AIType = ProjectileID.Shuriken;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            CreateWoodDusts(Projectile.position, Projectile.velocity);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            CreateWoodDusts(Projectile.position, Projectile.velocity);
            return true;
        }

        private void CreateWoodDusts(Vector2 pos, Vector2 vel)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustDirect(pos, Projectile.width, Projectile.height,
                    DustID.WoodFurniture, // 木屑 Dust
                    vel.X * 0.1f,
                    vel.Y * 0.1f,
                    100, default, 0.85f
                );
            }
        }
    }
}
