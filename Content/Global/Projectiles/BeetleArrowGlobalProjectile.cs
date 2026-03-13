using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Players;

namespace WuDao.Content.Global.Projectiles
{
    public class BeetleArrowGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[projectile.owner];

            var modPlayer = player.GetModPlayer<BeetleArrowPlayer>();

            if (!modPlayer.beetleArrow)
                return;

            if (projectile.DamageType != DamageClass.Summon)
                return;

            if (Main.rand.NextFloat() < 0.04f)
            {
                modifiers.SetCrit();
            }
        }
    }
}