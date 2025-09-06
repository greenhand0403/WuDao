using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Passive;

namespace WuDao.Content.Global.Projectiles
{
    // 拦截所有武器发射射弹：用于被动绝学“在发射射弹的同时随机触发”
    public class JuexueGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => false;

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            var qi = player.GetModPlayer<QiPlayer>();
            if (qi.JuexueSlot.ModItem is SharkWhaleFist sw)
            {
                sw.TryPassiveTriggerOnShoot(player, qi, source, position, velocity, type, damage, knockback);
            }
            if (qi.JuexueSlot.ModItem is HeavenlyPetals hp)
            {
                hp.TryPassiveTriggerOnShoot(player, qi, source, position, velocity, type, damage, knockback);
            }
            // —— 新被动：九阴白骨爪 —— 
            if (qi.JuexueSlot.ModItem is WhiteBoneClaw wbc)
                wbc.TryPassiveTriggerOnShoot(player, qi, source, position, velocity, type, damage, knockback);

            // —— 新被动：降龙十八掌 —— 
            if (qi.JuexueSlot.ModItem is XiangLong18 xl)
                xl.TryPassiveTriggerOnShoot(player, qi, source, position, velocity, type, damage, knockback);
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
    }
}
