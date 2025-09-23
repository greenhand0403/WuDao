using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Common;
using System;
using Terraria.Utilities;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Juexue.Active
{
    // TODO: 剑之虚影
    public class TenThousandSwords : JuexueItem
    {
        public override int QiCost => 200;
        public override int SpecialCooldownTicks => 60 * 2; // 2 秒

        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            var rect = Helpers.ScreenBoundsWorldSpace();
            Vector2 mouse = Main.MouseWorld;

            int count = Main.rand.Next(16, 24);
            float vnum = 5f;
            for (int i = 0; i < count; i++)
            {
                // 随机边缘
                int edge = Main.rand.Next(4);
                Vector2 spawn = edge switch
                {
                    0 => new Vector2(rect.Left, Main.rand.Next(rect.Top, rect.Bottom)),     // 左
                    1 => new Vector2(rect.Right, Main.rand.Next(rect.Top, rect.Bottom)),    // 右
                    2 => new Vector2(Main.rand.Next(rect.Left, rect.Right), rect.Top),      // 上
                    _ => new Vector2(Main.rand.Next(rect.Left, rect.Right), rect.Bottom)    // 下
                };
                Vector2 v = (mouse - spawn).SafeNormalize(Vector2.UnitX) * vnum * Main.rand.NextFloat(1.2f, 1.8f);

                int projType = ModContent.ProjectileType<TenThousandSwordsProj>();
                int proj = Projectile.NewProjectile(player.GetSource_ItemUse(Item), spawn, v, projType, 80, 2f, player.whoAmI);
                if (proj>=0)
                {
                    Main.projectile[proj].penetrate = 20; // 高穿透
                    Main.projectile[proj].tileCollide = false;
                    Main.projectile[proj].netUpdate = true;
                }
            }
            return true;
        }
    }
}
