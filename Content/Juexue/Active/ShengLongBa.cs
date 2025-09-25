// 庐山升龙霸：100 气，从光标位置向上飞出 8 条“飞龙”投射物（占位 Betsy 火球），高穿透。
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Projectiles;

namespace WuDao.Content.Juexue.Active
{
    public class ShengLongBa : JuexueItem
    {
        public override bool IsActive => true;
        public override int QiCost => 50;
        public override int SpecialCooldownTicks => 60 * 30; // 30s
        public const int ShengLongBaFrameIndex = 0;
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            // if (!qi.TrySpendQi(QiCost)) { Main.NewText("气力不足！", Color.OrangeRed); return false; }

            Vector2 at = Main.MouseWorld + new Vector2(0, 64f);
            int damage = Helpers.BossProgressPower.GetUniqueBossCount() * 50;
            for (int i = 0; i < 1; i++)
            {
                Vector2 v = new Vector2(Main.rand.NextFloat(-2.2f, 2.2f), -Main.rand.NextFloat(12f, 24f));
                int proj = Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    at,
                    v,
                    ModContent.ProjectileType<WyvernCompositeProjectile>(),
                    damage,
                    3f,
                    player.whoAmI);
                var p = Main.projectile[proj];
            }
            if (!Main.dedServ)
            {
                // 触发 2 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
                qi.TriggerJuexueGhost(ShengLongBaFrameIndex, durationTick: 60, scale: 1.1f, offset: new Vector2(0, -20));
            }
            return true;
        }
    }
}
