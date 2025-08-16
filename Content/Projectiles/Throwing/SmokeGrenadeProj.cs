using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace WuDao.Content.Projectiles.Ranged
{
    // 烟雾弹 射弹
    public class SmokeGrenadeProj : ModProjectile
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("烟雾手雷");
        // }
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Grenade}";
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180; // 3秒内若没撞也会自爆
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f * Projectile.direction;
            Projectile.velocity.Y += 0.25f; // 重力
            if (Main.rand.NextBool(4))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0, 0, 150, default, 1.2f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // 生成烟雾场
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<SmokeCloud>(), 1, 0f, Projectile.owner, ai0: 120f /*2秒*/);
            for (int i = 0; i < 20; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2), 100, default, 2.5f);
            SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
        }
    }
    // 烟雾弹 滞留烟雾射弹
    public class SmokeCloud : ModProjectile
    {
        private int _tick;
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("烟雾");
        // }
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.ToxicCloud}";
        public override void SetDefaults()
        {
            Projectile.width = 160;  // 烟雾直径约 10 格
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120; // 2秒
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            // if (Projectile.ai[0] > 0) Projectile.timeLeft = (int)Projectile.ai[0];

            // 漫延烟雾粉尘
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2);
                Dust.NewDustPerfect(pos, DustID.Smoke, Vector2.Zero, 200, default, Main.rand.NextFloat(2.2f, 2.8f));
            }

            // 每 10 tick 结算一次 DOT：5~10 固定伤害
            if (++_tick >= 10)
            {
                _tick = 0;
                foreach (var npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && npc.life > 0)
                    {
                        if (Collision.CheckAABBvAABBCollision(npc.position, npc.Size, Projectile.position, new Vector2(Projectile.width, Projectile.height)))
                        {
                            int dmg = Main.rand.Next(5, 11);
                            var hitInfo = new NPC.HitInfo
                            {
                                Damage = dmg,
                                Knockback = 0f,
                                HitDirection = npc.direction,
                                Crit = false
                            };
                            npc.StrikeNPC(hitInfo, fromNet: false);
                            // 轻微特效
                            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Smoke, 0, -1f, 120, default, 2f);
                        }
                    }
                }
            }
        }
    }
}
