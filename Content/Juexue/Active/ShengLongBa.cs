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
    // TODO: 庐山升龙霸，青龙虚影
    public class ShengLongBa : JuexueItem
    {
        public override bool IsActive => true;
        public override int QiCost => 10;
        public override int SpecialCooldownTicks => 60 * 1; // 1s

        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            if (!qi.TrySpendQi(QiCost)) { Main.NewText("气力不足！", Color.OrangeRed); return false; }

            Vector2 at = Main.MouseWorld + new Vector2(0, 48f);
            for (int i = 0; i < 1; i++)
            {
                Vector2 v = new Vector2(Main.rand.NextFloat(-2.2f, 2.2f), -Main.rand.NextFloat(12f, 24f));
                int proj = Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    at,
                    v,
                    ModContent.ProjectileType<WyvernCompositeProjectile>(),
                    95,
                    3f,
                    player.whoAmI);
                var p = Main.projectile[proj];
            }
            return true;
        }
    }
}
