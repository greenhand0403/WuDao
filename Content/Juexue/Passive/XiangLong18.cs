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
using WuDao.Common;

namespace WuDao.Content.Juexue.Passive
{
    public class XiangLong18 : JuexueItem
    {
        public override bool IsActive => false;
        public const int Cost = 10;
        public const float Chance = 0.4f; // 可按需调整
        public const int XiangLong18FrameIndex = 8;
        public void TryPassiveTriggerOnShoot(Player player, QiPlayer qi, EntitySource_ItemUse_WithAmmo src,
            Vector2 pos, Vector2 vel, int type, int dmg, float kb)
        {
            if (qi.QiMax <= 0) return;
            if (Main.rand.NextFloat() > Chance) return;
            if (!qi.TrySpendQi(Cost)) return;

            qi.XiangLongCount++;
            float mult = 1f;
            if (qi.XiangLongCount == 8) mult = 8f;  // 800%
            if (qi.XiangLongCount == 10) { mult = 10f; qi.XiangLongCount = 0; } // 1000% then reset

            Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 12f;
            int damage = (int)(dmg * mult) + 30 * Helpers.BossProgressPower.GetUniqueBossCount();
            // 略微下移一点对齐发射口
            int proj = Projectile.NewProjectile(src, player.Center + Vector2.UnitY * 8, dir, ModContent.ProjectileType<PhantomDragonProjectile>(), damage, kb + 2f, player.whoAmI);
            Main.projectile[proj].DamageType = DamageClass.Melee;
            Main.projectile[proj].tileCollide = false;
            Main.projectile[proj].penetrate = -1;
            Main.projectile[proj].timeLeft = 180;

            if (!Main.dedServ)
            {
                // 触发 2 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
                qi.TriggerJuexueGhost(XiangLong18FrameIndex, durationTick: 120, scale: 1.1f, offset: new Vector2(0, -20));
            }
        }
    }
}
