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
    // 杀鲸霸拳
    public class SharkWhaleFist : JuexueItem
    {
        public override bool IsActive => false; // 被动
        public override int QiCost => 15;
        public const float Chance = 0.25f; // 随机触发几率（与发射射弹同步
        public const int SharkWhaleFistFrameIndex = 11;
        public const int baseDamge = 51;// 基础伤害
        // 新增：被动触发冷却（单位tick）复用主动技能冷却
        public override int SpecialCooldownTicks => 60;
        public void TryPassiveTriggerOnShoot(Player player, QiPlayer qi, EntitySource_ItemUse_WithAmmo src,
            Vector2 pos, Vector2 vel, int type, int damage, float knockback)
        {
            if (Main.rand.NextFloat() > Chance) return;
            // ★ 冷却检查（在消耗气力之前）
            if (!qi.CanProcPassiveNow(Item.type, SpecialCooldownTicks)) return;
            if (!qi.TrySpendQi(QiCost)) return;
            // ★ 通过后立刻盖章，避免同一帧多枚弹丸连触发
            qi.StampPassiveProc(Item.type, SpecialCooldownTicks);

            int projType = ModContent.ProjectileType<SharkProjectile>();

            // 计算境界伤害和射弹速度加成
            Helpers.BossProgressBonus progressBonus = Helpers.BossProgressPower.Get(player);
            // 沿用发射时射弹的初速度
            Vector2 v = vel.SafeNormalize(Vector2.UnitX) * vel.Length() * progressBonus.ProjSpeedMult;
            int projDamage = (int)(baseDamge * progressBonus.DamageMult);
            // 25%转化为鲸鱼射弹
            if (Main.rand.NextBool(4))
            {
                projType = ModContent.ProjectileType<OrcaProjectile>();
                projDamage *= 3;
            }
            Vector2 spawnPos = pos + v.SafeNormalize(Vector2.UnitX) * 32f; // 固定往前前推 32 像素
            int proj = Projectile.NewProjectile(src, spawnPos, v, projType, projDamage, knockback, player.whoAmI);
            if (proj < 0) return;
            Main.projectile[proj].friendly = true;
            Main.projectile[proj].hostile = false;

            // —— 启动“鲨鱼虚影” —— //
            if (!Main.dedServ)
            {
                // 触发 2 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
                qi.TriggerJuexueGhost(SharkWhaleFistFrameIndex, durationTick: 45, scale: 1.1f, offset: new Vector2(0, -20));
            }
        }
    }
}
