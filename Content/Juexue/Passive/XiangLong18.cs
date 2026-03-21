using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Projectiles.Melee;
using WuDao.Common;
using WuDao.Content.DamageClasses;

namespace WuDao.Content.Juexue.Passive
{
    // 降龙十八掌：被动触发消耗气，向光标发出“飞龙”投射物；第8/10次大幅增伤，之后归零。
    // 贴图占位：Betsy Fireball
    public class XiangLong18 : JuexueItem
    {
        public override bool IsActive => false;
        public override int QiCost => 30;
        public const float Chance = 0.25f; // 可按需调整
        public const int XiangLong18FrameIndex = 8;
        public const int baseDamage = 560;
        // 新增：被动触发冷却（单位tick）复用主动技能冷却
        public override int SpecialCooldownTicks => 60;
        public void TryPassiveTriggerOnShoot(Player player, QiPlayer qi, EntitySource_ItemUse_WithAmmo src,
            Vector2 pos, Vector2 vel, int type, int dmg, float kb)
        {
            // 只允许技能拥有者本人生成这颗射弹
            if (player.whoAmI != Main.myPlayer)
                return;
                
            if (Main.rand.NextFloat() > Chance) return;
            // ★ 冷却检查（在消耗气力之前）
            if (!qi.CanProcPassiveNow(Item.type)) return;
            if (!qi.TrySpendQi(QiCost)) return;
            // ★ 通过后立刻盖章，避免同一帧多枚弹丸连触发
            qi.StampPassiveProc(Item.type, SpecialCooldownTicks);

            qi.XiangLongCount++;
            float mult = 1f;
            if (qi.XiangLongCount == 8) mult = 8f;  // 800%
            if (qi.XiangLongCount == 18) { mult = 18f; qi.XiangLongCount = 0; } // 1000% then reset

            // 计算武道境界伤害和射弹速度加成
            Helpers.BossProgressBonus progressBonus = Helpers.BossProgressPower.Get(player);
            Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * vel.Length() * progressBonus.ProjSpeedMult;

            ChiEnergyDamageClass chi = ModContent.GetInstance<ChiEnergyDamageClass>();
            int finalDamage = (int)(player.GetTotalDamage(chi).ApplyTo(baseDamage) * mult * progressBonus.DamageMult);

            // 略微下移一点对齐发射口
            int proj = Projectile.NewProjectile(
                src,
                player.Center + Vector2.UnitY * 8,
                dir,
                ModContent.ProjectileType<PhantomDragonProjectile>(),
                finalDamage,
                kb + 2f,
                player.whoAmI
            );
            Main.projectile[proj].DamageType = chi;
            Main.projectile[proj].originalDamage = finalDamage;
            Main.projectile[proj].netUpdate = true;
            if (!Main.dedServ)
            {
                // 触发 2 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
                qi.TriggerJuexueGhost(XiangLong18FrameIndex, durationTick: 45, scale: 1.1f, offset: new Vector2(0, -20));
            }
        }
    }
}
