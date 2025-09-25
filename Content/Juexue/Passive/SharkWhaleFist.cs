using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Common;
using WuDao.Content.Projectiles.Melee;
using WuDao.Content.Projectiles.Magic;
using Microsoft.Xna.Framework.Graphics;

namespace WuDao.Content.Juexue.Passive
{
    public class SharkWhaleFist : JuexueItem
    {
        public override bool IsActive => false; // 被动
        public const int Cost = 5;
        public const float Chance = 0.25f; // 随机触发几率（与发射射弹同步
        public const int SharkWhaleFistFrameIndex = 11;
        public void TryPassiveTriggerOnShoot(Player player, QiPlayer qi, EntitySource_ItemUse_WithAmmo src,
            Vector2 pos, Vector2 vel, int type, int damage, float knockback)
        {

            if (qi.QiMax <= 0) return;
            if (Main.rand.NextFloat() > Chance) return;
            if (!qi.TrySpendQi(Cost)) return;

            int projType = Main.rand.NextBool(4)
                ? ModContent.ProjectileType<OrcaProjectile>()  // “鲸”占位：更显眼的碎片
                : ModContent.ProjectileType<SharkProjectile>(); // “鲨”占位：鲨弹

            Vector2 v = vel.SafeNormalize(Vector2.UnitX) * vel.Length(); // 同向
            int projDamage = (int)(damage * 0.9f) + 20 * Helpers.BossProgressPower.GetUniqueBossCount();
            int proj = Projectile.NewProjectile(src, pos + 5 * v, v, projType, projDamage, knockback, player.whoAmI);
            if (proj < 0) return;
            Main.projectile[proj].friendly = true;
            Main.projectile[proj].hostile = false;

            // —— 启动“鲨鱼虚影” —— //
            if (!Main.dedServ)
            {
                // 触发 2 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
                qi.TriggerJuexueGhost(SharkWhaleFistFrameIndex, durationTick: 120, scale: 1.1f, offset: new Vector2(0, -20));
            }
        }
    }
}
