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
            // 总开关 + 白名单判定
            if (BladeTrailRuntime.IsAllowed(item))
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
                        player.whoAmI,
                        ai0: item.useAnimation   // 传给弹幕
                    );
                }
            }
            return base.UseItem(item, player);
        }

        public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // 只对白名单近战、且当前有我的刀光时，禁用原版近战 hitbox
            if (BladeTrailRuntime.IsAllowed(item)
                && item.DamageType.CountsAsClass(DamageClass.Melee)
                && !item.noMelee
                && player.GetModPlayer<BladeTrailPlayer>().TrailActive)
            {
                noHitbox = true; // 原版近战不再造成伤害
            }
        }
    }
}
