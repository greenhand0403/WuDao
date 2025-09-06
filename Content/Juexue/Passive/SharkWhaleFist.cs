using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Common;
using WuDao.Content.Projectiles.Ranged;

namespace WuDao.Content.Juexue.Passive
{
    // TODO: 增加鲸射弹和鲨射弹
    public class SharkWhaleFist : JuexueItem
    {
        public override bool IsActive => false; // 被动
        public override JuexueID JuexueId => JuexueID.Passive_SharkWhaleFist;

        public const int Cost = 5;
        public const float Chance = 0.33f; // 随机触发几率（与发射射弹同步）

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("绝学·鲨鲸霸拳");
            // Tooltip.SetDefault("被动：攻击发射射弹时有几率追加“鲨/鲸”弹幕（消耗5点气力；暴击用‘鲸’替代）。");
        }

        public void TryPassiveTriggerOnShoot(Player player, QiPlayer qi, EntitySource_ItemUse_WithAmmo src,
            Vector2 pos, Vector2 vel, int type, int damage, float knockback)
        {

            if (qi.QiMax <= 0) return;
            if (Main.rand.NextFloat() > Chance) return;
            if (!qi.TrySpendQi(Cost)) return;

            int projType = player.GetCritChance(DamageClass.Generic) > 0 && Main.rand.NextFloat() < player.GetTotalCritChance(DamageClass.Generic) / 100f
                ? ModContent.ProjectileType<BrightVerdictProjectile>()  // “鲸”占位：更显眼的碎片
                : ProjectileID.WoodenArrowFriendly; // “鲨”占位：沙鲨弹

            Vector2 v = vel.SafeNormalize(Vector2.UnitX) * vel.Length(); // 同向
            int proj = Projectile.NewProjectile(src, pos, v, projType, (int)(damage * 0.9f), knockback, player.whoAmI);
            if (proj < 0) return;
            Main.projectile[proj].friendly = true;
            Main.projectile[proj].hostile = false;
        }
    }
}
