using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.DamageClasses;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;

namespace WuDao.Content.Juexue.Passive
{
    // 天女散花
    public class HeavenlyPetals : JuexueItem
    {
        public override bool IsActive => false; // 被动
        public const int HeavenlyPetalsFrameIndex = 1;
        public const int Cost = 15;
        public const float Chance = 0.30f; // 30%
        public const int baseDamage = 176;

        public void TryPassiveTriggerOnShoot(Player player, QiPlayer qi, EntitySource_ItemUse_WithAmmo src,
            Vector2 pos, Vector2 vel, int type, int damage, float knockback)
        {
            // 只允许技能拥有者本人生成这颗射弹
            if (player.whoAmI != Main.myPlayer)
                return;

            if (qi.QiMax <= 0) return;
            if (Main.rand.NextFloat() > Chance) return;
            if (!qi.TrySpendQi(Cost)) return;

            ChiEnergyDamageClass chi = ModContent.GetInstance<ChiEnergyDamageClass>();
            int finalDamage = (int)player.GetTotalDamage(chi).ApplyTo(baseDamage);

            int projType = ProjectileID.FlowerPowPetal;
            // Vector2 mouse = Main.MouseWorld;
            int count = Main.rand.Next(6, 13);
            for (int i = 0; i < count; i++)
            {
                Vector2 spawn = player.Center + Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / count);
                // Vector2 dir = (mouse - spawn).SafeNormalize(Vector2.UnitX) * 12f;
                Vector2 dir = Vector2.One.RotatedBy(MathHelper.TwoPi * i / count);
                int proj = Projectile.NewProjectile(src, spawn, dir * 5f, projType, finalDamage, knockback, player.whoAmI);
                Main.projectile[proj].DamageType = chi;
                Main.projectile[proj].originalDamage = finalDamage;
            }

            // —— 启动“花瓣虚影” —— //
            if (!Main.dedServ)
            {
                // 触发 2 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
                qi.TriggerJuexueGhost(HeavenlyPetalsFrameIndex, durationTick: 30, scale: 1.1f, offset: new Vector2(0, -20));
            }
        }
    }
}
