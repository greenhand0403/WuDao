using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Projectiles;
using WuDao.Content.DamageClasses;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Juexue.Active
{
    // TODO: 无形剑气：向光标位置发射 1 条“无形剑气”投射物，穿透。
    public class InvisibleSwordQi : JuexueItem
    {
        public override bool IsActive => true;
        public override int QiCost => 0;
        public override int SpecialCooldownTicks => 0; // 45s
        public const int ShengLongBaFrameIndex = 0;
        public const int baseDamage = 255;// 基础伤害
        public const int baseVelocity = 6;// 基础速度
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            // 计算武道境界伤害和射弹速度加成
            Helpers.BossProgressBonus progressBonus = Helpers.BossProgressPower.Get(player);

            ChiEnergyDamageClass chi = ModContent.GetInstance<ChiEnergyDamageClass>();
            int finalDamage = (int)(player.GetTotalDamage(chi).ApplyTo(baseDamage) * progressBonus.DamageMult);

            // 从玩家位置指向光标位置，计算速度
            Vector2 v = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero) ;
            int proj = Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    player.Center + v * 72f,// 从玩家位置发射,朝目标方向偏移一点点
                    v * baseVelocity,
                    ModContent.ProjectileType<InvisibleSwordQiProj>(),
                    finalDamage,
                    3f,
                    player.whoAmI);
            var p = Main.projectile[proj];
            p.DamageType = chi;
            p.originalDamage = finalDamage;

            if (!Main.dedServ)
            {
                // 冷却图标
                qi.TriggerJuexueCooldownIcon(
                    frameIndex: ShengLongBaFrameIndex,
                    itemType: Type,                    // ModItem 的 Type
                    cooldownTicks: SpecialCooldownTicks,
                    scale: 1.1f,
                    offset: new Vector2(0, -20)
                );
            }
            return true;
        }
    }
}
