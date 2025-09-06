// 庐山升龙霸：100 气，从光标位置向上飞出 8 条“飞龙”投射物（占位 Betsy 火球），高穿透。
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;

namespace WuDao.Content.Juexue.Active
{
    // TODO: 庐山升龙霸，补贴图
    public class ShengLongBa : JuexueItem
    {
        public override JuexueID JuexueId => JuexueID.Active_ShengLongBa;
        public override bool IsActive => true;
        public override int QiCost => 100;
        public override int SpecialCooldownTicks => 60 * 12; // 12s

        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            if (!qi.TrySpendQi(QiCost)) { Main.NewText("气力不足！", Microsoft.Xna.Framework.Color.OrangeRed); return false; }

            Vector2 at = Main.MouseWorld + new Vector2(0, 48f);
            for (int i = 0; i < 8; i++)
            {
                Vector2 v = new Vector2(Main.rand.NextFloat(-2.2f, 2.2f), -Main.rand.NextFloat(16f, 22f));
                int proj = Projectile.NewProjectile(player.GetSource_ItemUse(Item), at, v,
                    ProjectileID.DD2BetsyFireball, 95, 3f, player.whoAmI);
                var p = Main.projectile[proj];
                p.tileCollide = false;
                p.penetrate = -1; // 无限穿透
                p.timeLeft = 180;
                p.friendly = true;
                p.hostile = false;
            }
            return true;
        }
    }
}
