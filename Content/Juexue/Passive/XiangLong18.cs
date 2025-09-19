// 降龙十八掌：被动触发消耗10气，向光标发出“飞龙”投射物；第8/10次大幅增伤，之后归零。
// 贴图占位：Betsy Fireball
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Juexue.Passive
{
    // TODO: 黄龙虚影
    public class XiangLong18 : JuexueItem
    {
        public override bool IsActive => false;
        public const int Cost = 10;
        public const float Chance = 0.90f; // 可按需调整

        public void TryPassiveTriggerOnShoot(Player player, QiPlayer qi, EntitySource_ItemUse_WithAmmo src,
            Vector2 pos, Vector2 vel, int type, int dmg, float kb)
        {
            if (qi.QiMax <= 0) return;
            if (Main.rand.NextFloat() > Chance) return;
            if (!qi.TrySpendQi(Cost)) return;

            qi.XiangLongCount++;
            float mult = 1f;
            if (qi.XiangLongCount == 8) mult = 9f;  // +800%
            if (qi.XiangLongCount == 10) { mult = 11f; qi.XiangLongCount = 0; } // +1000% then reset

            Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 12f;
            int proj = Projectile.NewProjectile(src, player.Center, dir, ModContent.ProjectileType<PhantomDragonProjectile>(),
                (int)(dmg * mult), kb + 2f, player.whoAmI);
            Main.projectile[proj].DamageType = DamageClass.Melee;
            Main.projectile[proj].tileCollide = false;
            Main.projectile[proj].penetrate = -1;
            Main.projectile[proj].timeLeft = 180;
        }
    }
}
