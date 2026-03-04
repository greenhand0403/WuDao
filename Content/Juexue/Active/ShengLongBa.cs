using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Projectiles;
using WuDao.Content.DamageClasses;

namespace WuDao.Content.Juexue.Active
{
    // 庐山升龙霸：从光标位置向上飞出 8 条“飞龙”投射物（占位 Betsy 火球），高穿透。
    public class ShengLongBa : JuexueItem
    {
        public override bool IsActive => true;
        public override int QiCost => 60;
        public override int SpecialCooldownTicks => 60 * 45; // 45s
        public const int ShengLongBaFrameIndex = 0;
        public const int baseDamage = 255;// 基础伤害
        public const int baseVelocity = 14;// 基础速度
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            Vector2 at = Main.MouseWorld + new Vector2(0, 200f);
            // 计算境界伤害和射弹速度加成
            Helpers.BossProgressBonus progressBonus = Helpers.BossProgressPower.Get(player);
            
            ChiEnergyDamageClass chi = ModContent.GetInstance<ChiEnergyDamageClass>();
            int finalDamage = (int)(player.GetTotalDamage(chi).ApplyTo(baseDamage)* progressBonus.DamageMult);
            
            Vector2 v = Vector2.UnitY * -baseVelocity;
            for (int i = 0; i < 1; i++)
            {
                int proj = Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    at,
                    v,
                    ModContent.ProjectileType<WyvernCompositeProjectile>(),
                    finalDamage,
                    3f,
                    player.whoAmI);
                var p = Main.projectile[proj];
                p.DamageType = chi;
                p.originalDamage = finalDamage;
            }
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
