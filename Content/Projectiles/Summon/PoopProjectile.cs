using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Summon;

public class PoopProjectile : ModProjectile
{
    public override string Texture => "Terraria/Images/Item_"+ItemID.PoopBlock;
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Summon;

        Projectile.timeLeft = 200;
    }

    public override void AI()
    {
        // 重力
        Projectile.velocity.Y += 0.2f;

        // 风扰动
        Projectile.velocity.X += Main.rand.NextFloat(-0.03f, 0.03f);

        Projectile.rotation += Projectile.velocity.X * 0.1f;
    }
}