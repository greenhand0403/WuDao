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
    // 万剑归宗
    public class TenThousandSwords : JuexueItem
    {
        public override int QiCost => 90;
        public override int SpecialCooldownTicks => 60 * 60; // 1 分钟
        public const int TenThousandSwordsFrameIndex = 5;
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            var rect = Helpers.ScreenBoundsWorldSpace();
            Vector2 mouse = Main.MouseWorld;

            int count = Main.rand.Next(12, 20);
            float vnum = 5f;
            int damage = 560;//Helpers.BossProgressPower.GetUniqueBossCount() * 50;
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
                Vector2 v = (mouse - spawn).SafeNormalize(Vector2.UnitX) * vnum * Main.rand.NextFloat(1.4f, 2.2f);

                int projType = ModContent.ProjectileType<TenThousandSwordsProj>();
                int proj = Projectile.NewProjectile(player.GetSource_ItemUse(Item), spawn, v, projType, damage, 2f, player.whoAmI);
                if (proj >= 0)
                {
                    Main.projectile[proj].penetrate = 20; // 高穿透
                    Main.projectile[proj].tileCollide = false;
                    Main.projectile[proj].netUpdate = true;
                }
            }
            if (!Main.dedServ)
            {
                // 冷却图标
                qi.TriggerJuexueCooldownIcon(
                    frameIndex: TenThousandSwordsFrameIndex,
                    itemType: Type,                    // ModItem 的 Type
                    cooldownTicks: SpecialCooldownTicks,
                    scale: 1.1f,
                    offset: new Vector2(0, -20)
                );
            }
            return true;
        }
    }
}
