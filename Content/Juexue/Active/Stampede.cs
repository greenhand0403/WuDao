// 万马奔腾：100 气，从远离鼠标的一侧屏幕边生成多枚投射物横穿屏幕。
// 贴图占位：EnchantedBeam（后续可换成自绘的独角兽/骏马投射物）。
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;

namespace WuDao.Content.Juexue.Active
{
    // TODO: 万马奔腾，补射弹
    public class Stampede : JuexueItem
    {
        public override JuexueID JuexueId => JuexueID.Active_Stampede;
        public override bool IsActive => true;
        public override int QiCost => 100;
        public override int SpecialCooldownTicks => 60 * 15; // 15s

        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            if (!qi.TrySpendQi(QiCost)) { Main.NewText("气力不足！", Microsoft.Xna.Framework.Color.OrangeRed); return false; }

            var rect = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
            Vector2 mouse = Main.MouseWorld;

            bool mouseOnRight = mouse.X > rect.Center.X;
            int startX = mouseOnRight ? rect.Left - 40 : rect.Right + 40;
            int targetX = mouseOnRight ? rect.Right + 200 : rect.Left - 200;
            int count = Main.rand.Next(26, 34);

            for (int i = 0; i < count; i++)
            {
                int y = Main.rand.Next(rect.Top + 40, rect.Bottom - 40);
                Vector2 spawn = new Vector2(startX, y);
                Vector2 dir = (new Vector2(targetX, y + Main.rand.Next(-40, 40)) - spawn).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(14f, 19f);

                int proj = Projectile.NewProjectile(player.GetSource_ItemUse(Item), spawn, dir,
                    ProjectileID.EnchantedBeam, 85, 2f, player.whoAmI);
                var p = Main.projectile[proj];
                p.tileCollide = false;
                p.penetrate = 20;
                p.timeLeft = 150;
            }
            return true;
        }
    }
}
