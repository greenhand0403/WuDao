using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Common;

namespace WuDao.Content.Juexue.Passive
{
    // TODO: 增加花瓣射弹
    public class HeavenlyPetals : JuexueItem
    {
        public override bool IsActive => false; // 被动
        public override JuexueID JuexueId => JuexueID.Passive_HeavenlyPetals;

        public const int Cost = 10;
        public const float Chance = 0.30f; // 30%

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("绝学·天女散花");
            // Tooltip.SetDefault("被动：攻击发射射弹时有30%几率在周围产生6道花瓣射弹朝光标飞行（消耗10点气力）。");
        }

        public void TryPassiveTriggerOnShoot(Player player, QiPlayer qi, EntitySource_ItemUse_WithAmmo src,
            Vector2 pos, Vector2 vel, int type, int damage, float knockback)
        {

            if (qi.QiMax <= 0) return;
            if (Main.rand.NextFloat() > Chance) return;
            if (!qi.TrySpendQi(Cost)) return;

            // 原版占位弹幕：花瓣可用花叶/水晶碎片等
            int projType = ProjectileID.CrystalLeafShot;
            Vector2 mouse = Main.MouseWorld;
            float count = 12.0f;
            for (int i = 0; i < 12; i++)
            {
                Vector2 spawn = player.Center + Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / count) * 40f;
                // Vector2 dir = (mouse - spawn).SafeNormalize(Vector2.UnitX) * 12f;
                Vector2 dir = Vector2.One.RotatedBy(MathHelper.TwoPi * i / count);
                Projectile.NewProjectile(src, spawn, dir, projType, (int)(damage * 0.8f), knockback, player.whoAmI);
            }
        }
    }
}
