using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Passive;
using WuDao.Content.Config;

namespace WuDao.Content.Global.Projectiles
{
    public class JuexueGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => false;

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (!JuexueRuntime.Enabled)
                return base.Shoot(item, player, source, position, velocity, type, damage, knockback);

            // 只让拥有者执行一次，避免多人重复触发被动附加射弹
            if (player.whoAmI != Main.myPlayer)
                return base.Shoot(item, player, source, position, velocity, type, damage, knockback);

            var qi = player.GetModPlayer<QiPlayer>();

            if (qi.JuexueSlot.ModItem is SharkWhaleFist sw)
                sw.TryPassiveTriggerOnShoot(player, qi, source, position, velocity, type, damage, knockback);

            if (qi.JuexueSlot.ModItem is HeavenlyPetals hp)
                hp.TryPassiveTriggerOnShoot(player, qi, source, position, velocity, type, damage, knockback);

            if (qi.JuexueSlot.ModItem is WhiteBoneClaw wbc)
                wbc.TryPassiveTriggerOnShoot(player, qi, source, position, velocity, type, damage, knockback);

            if (qi.JuexueSlot.ModItem is XiangLong18 xl)
                xl.TryPassiveTriggerOnShoot(player, qi, source, position, velocity, type, damage, knockback);

            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
    }
}