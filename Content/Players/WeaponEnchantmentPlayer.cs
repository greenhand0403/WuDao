using Terraria;
using Terraria.ModLoader;
using WuDao.Content.Buffs;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Players
{
    // 凝胶瓶的武器灌注效果，凝胶溅射
    class WeaponEnchantmentPlayer : ModPlayer
    {
        public bool GelFlaskImbue;
        public override void ResetEffects()
        {
            GelFlaskImbue = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (GelFlaskImbue && item.DamageType.CountsAsClass<MeleeDamageClass>())
            {
                target.AddBuff(ModContent.BuffType<GelFlaskDebuff>(), 60 * Main.rand.Next(3, 7));
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (GelFlaskImbue && (proj.DamageType.CountsAsClass<MeleeDamageClass>() || ProjectileID.Sets.IsAWhip[proj.type]) && !proj.noEnchantments)
            {
                target.AddBuff(ModContent.BuffType<GelFlaskDebuff>(), 60 * Main.rand.Next(3, 7));
            }
        }

        // MeleeEffects and EmitEnchantmentVisualsAt apply the visual effects of the weapon imbue to items and projectiles respectively.
        public override void MeleeEffects(Item item, Rectangle hitbox)
        {
            if (GelFlaskImbue && item.DamageType.CountsAsClass<MeleeDamageClass>() && !item.noMelee && !item.noUseGraphic)
            {
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Flare);
                    dust.velocity *= 0.5f;
                }
            }
        }

        public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
        {
            if (GelFlaskImbue && (projectile.DamageType.CountsAsClass<MeleeDamageClass>() || ProjectileID.Sets.IsAWhip[projectile.type]) && !projectile.noEnchantments)
            {
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustDirect(boxPosition, boxWidth, boxHeight, DustID.Water);
                    dust.velocity *= 0.5f;
                }
            }
        }
    }
}