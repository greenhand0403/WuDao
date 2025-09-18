// 万马奔腾：100 气，从远离鼠标的一侧屏幕边生成多枚投射物横穿屏幕。
// 贴图占位：EnchantedBeam（后续可换成自绘的独角兽/骏马投射物）。
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Juexue.Active
{
    // TODO: 万马奔腾，补射弹，马虚影
    public class Stampede : JuexueItem
    {
        public override JuexueID JuexueId => JuexueID.Active_Stampede;
        public override bool IsActive => true;
        public override int QiCost => 100;
        public override int SpecialCooldownTicks => 60 * 15; // 15s

        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            if (!qi.TrySpendQi(QiCost))
            {
                Main.NewText("气力不足！", Microsoft.Xna.Framework.Color.OrangeRed);
                return false;
            }

            // 屏幕矩形（世界坐标）
            Rectangle rect = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);

            // 从“远离鼠标”的一侧生成，横穿到对侧
            bool mouseOnRight = Main.MouseWorld.X > rect.Center.X;
            int startX = mouseOnRight ? rect.Left - 100 : rect.Right + 100;  // 出生点在屏幕外 100px
            int targetX = mouseOnRight ? rect.Right + 320 : rect.Left - 320;  // 终点再飞出屏 320px

            // 本轮数量：10~15
            int count = Main.rand.Next(10, 16);

            // 只在本地玩家侧生成一次控制器；由它“陆续”生成马
            if (player.whoAmI == Main.myPlayer)
            {
                int idx = Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    player.Center, // 控制器自身位置无所谓
                    Vector2.Zero,
                    ModContent.ProjectileType<StampedeSpawnerProj>(),
                    0, 0f, player.whoAmI
                );
                if (idx >= 0 && Main.projectile[idx].ModProjectile is StampedeSpawnerProj sp)
                {
                    sp.Setup(rect, startX, targetX, count, damage: 85, knockback: 2f);
                }
            }

            return true;
        }
    }
}
