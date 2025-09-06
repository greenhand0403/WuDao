using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Common;
using System;
using Terraria.Utilities;

namespace WuDao.Content.Juexue.Active
{
    // TODO: 增加剑类射弹
    public class TenThousandSwords : JuexueItem
    {
        public override JuexueID JuexueId => JuexueID.Active_TenThousandSwords;
        public override int QiCost => 200;
        public override int SpecialCooldownTicks => 60 * 20; // 20 秒

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("绝学·万剑归宗");
            // Tooltip.SetDefault("主动（200气）：在屏幕边缘生成多把剑形射弹，向光标突进并可穿透。");
        }

        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            var rect = Helpers.ScreenBoundsWorldSpace();
            Vector2 mouse = Main.MouseWorld;

            int count = Main.rand.Next(16, 24);
            float vnum = 30f;
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
                Vector2 v = (mouse - spawn).SafeNormalize(Vector2.UnitX) * vnum * Main.rand.NextFloat(0.8f, 1.2f);
                // 占位剑气：Enchanted Beam / SwordBeam
                int projType = ProjectileID.EnchantedBeam;
                int proj = Projectile.NewProjectile(player.GetSource_ItemUse(Item), spawn, v, projType, 80, 2f, player.whoAmI);
                Main.projectile[proj].penetrate = 20; // 高穿透
                Main.projectile[proj].tileCollide = false;
            }
            return true;
        }
    }
}
