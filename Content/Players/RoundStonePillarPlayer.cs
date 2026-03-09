using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Players
{
    public class RoundStonePillarPlayer : ModPlayer
    {
        public bool roundStonePillar;

        public override void ResetEffects()
        {
            roundStonePillar = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!roundStonePillar)
                return;

            int projType = modifiers.DamageSource.SourceProjectileType;

            if (projType >= 0)
            {
                if (projType == ProjectileID.Boulder ||
                    projType == ProjectileID.SpikyBallTrap ||
                    projType == ProjectileID.PoisonDartTrap ||
                    projType == ProjectileID.SpearTrap ||
                    projType == ProjectileID.FlamethrowerTrap ||
                    projType == ProjectileID.FlamesTrap ||
                    projType == ProjectileID.VenomDartTrap)
                {
                    modifiers.FinalDamage *= 0.5f;
                }
            }
        }
    }
}