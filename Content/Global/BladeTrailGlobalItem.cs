using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Global
{
    public class BladeTrailGlobalItem : GlobalItem
    {
        public override bool? UseItem(Item item, Player player)
        {
            if (ItemSets.BladeTrailSet.Contains(item.type))
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    Projectile.NewProjectile(
                        player.GetSource_ItemUse(item),
                        player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<BladeTrailProj>(),
                        item.damage,
                        item.knockBack,
                        player.whoAmI
                    );
                }
            }
            return base.UseItem(item, player);
        }
    }
}