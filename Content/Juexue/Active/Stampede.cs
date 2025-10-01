// 万马奔腾：100 气，从远离鼠标的一侧屏幕边生成多枚投射物横穿屏幕
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
        public override int QiCost => 90;
        public override int SpecialCooldownTicks => 60 * 60;
        public const int StampedeFrameIndex = 7;
        public const int baseDamge = 460;// 基础伤害
        public const int baseVelocity = 20;// 基础速度
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            // 屏幕矩形（世界坐标）
            Rectangle rect = Helpers.ScreenBoundsWorldSpace();
            // 计算境界伤害和射弹速度加成
            Helpers.BossProgressBonus progressBonus = Helpers.BossProgressPower.Get(player);
            // 从“远离鼠标”的一侧生成，横穿到对侧
            bool mouseOnRight = Main.MouseWorld.X > rect.Center.X;
            int startX = mouseOnRight ? rect.Left - 30 : rect.Right + 30;  // 出生点在屏幕外 30px
            // 速度
            Vector2 v = Vector2.UnitX * (mouseOnRight ? 1 : -1) * baseVelocity * progressBonus.ProjSpeedMult;
            // 每列投射物数量4~5匹马
            int rowCount = Main.rand.Next(4, 6);
            // 生成骏马射弹的总数
            int count = Main.rand.Next(4, 6) * rowCount;
            int projDamage = (int)(baseDamge * progressBonus.DamageMult);
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
