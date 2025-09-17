using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Common;
using WuDao.Content.Projectiles.Ranged;
using WuDao.Content.Projectiles;

namespace WuDao.Content.Juexue.Passive
{
    // TODO: 增加鲸射弹和鲨射弹，鲸鱼虚影
    public class SharkWhaleFist : JuexueItem
    {
        public override bool IsActive => false; // 被动
        public override JuexueID JuexueId => JuexueID.Passive_SharkWhaleFist;

        public const int Cost = 5;
        public const float Chance = 0.93f; // 随机触发几率（与发射射弹同步）

        public void TryPassiveTriggerOnShoot(Player player, QiPlayer qi, EntitySource_ItemUse_WithAmmo src,
            Vector2 pos, Vector2 vel, int type, int damage, float knockback)
        {

            if (qi.QiMax <= 0) return;
            if (Main.rand.NextFloat() > Chance) return;
            if (!qi.TrySpendQi(Cost)) return;

            int projType = Main.rand.NextBool(2)
                ? ModContent.ProjectileType<UnicornProjectile>()  // “鲸”占位：更显眼的碎片
                : ModContent.ProjectileType<SharkProjectile>(); // “鲨”占位：鲨弹

            // Vector2 v = vel.SafeNormalize(Vector2.UnitX) * vel.Length(); // 同向
            Vector2 v = vel.SafeNormalize(Vector2.UnitX) * 10f; // 同向
            int proj = Projectile.NewProjectile(src, pos, v, projType, (int)(damage * 0.9f), knockback, player.whoAmI);
            if (proj < 0) return;
            Main.projectile[proj].friendly = true;
            Main.projectile[proj].hostile = false;
        }
    }
}
