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
using System;

namespace WuDao.Content.Juexue.Active
{
    public class Stampede : JuexueItem
    {
        public override bool IsActive => true;
        public override int QiCost => 99;
        public override int SpecialCooldownTicks => 60 * 120; // 2min
        public const int StampedeFrameIndex = 7;
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            // 屏幕矩形（世界坐标）
            Rectangle rect = Helpers.ScreenBoundsWorldSpace();

            // 从“远离鼠标”的一侧生成，横穿到对侧
            bool mouseOnRight = Main.MouseWorld.X > rect.Center.X;
            int startX = mouseOnRight ? rect.Left - 30 : rect.Right + 30;  // 出生点在屏幕外 30px
            // 速度
            Vector2 v = Vector2.UnitX * (mouseOnRight ? 16 : -16);
            // 每列投射物数量
            int rowCount = 5;
            int count = Main.rand.Next(4, 6) * rowCount;
            int projDamage = 70 * Helpers.BossProgressPower.GetUniqueBossCount();
            if (player.whoAmI == Main.myPlayer)
            {
                float spawnY;
                int j = 0;
                // 定位的行高
                float tmpHeight = rect.Height / (2 * rowCount);
                for (int i = 0; i < count; i++)
                {
                    // 列序号 1 表示第 1 列
                    if (i % rowCount == 0)
                    {
                        j++;
                    }
                    // 列的间隔
                    int spawnX = mouseOnRight ? startX - j * 120 : startX + j * 120;
                    // 行间隔 额外偏移 0.5 倍行高
                    spawnY = (int)Math.Round(
                        (double)(tmpHeight * (2 * (i % rowCount + 1) - j % 2))
                        ) + rect.Top - 0.5f * tmpHeight;
                    // 额外调整位置
                    spawnY += rect.Height / 2;
                    Vector2 spawn = new Vector2(spawnX, spawnY);

                    int idx = Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                        spawn + new Vector2(0, 82 + 60),
                        v,
                        ModContent.ProjectileType<HorseItemVariantProjectile>(),
                        projDamage,
                        3f,
                        player.whoAmI,
                        i
                    );
                }
            }
            if (!Main.dedServ)
            {
                // 触发 1 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
                qi.TriggerJuexueGhost(StampedeFrameIndex, durationTick: 120, scale: 1.1f, offset: new Vector2(0, -20));
            }
            return true;
        }
    }
}
