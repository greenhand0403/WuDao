using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Common;
using Microsoft.Xna.Framework.Input;

namespace WuDao.Content.Juexue.Active
{
    // TODO: 增加磁场天刀射弹的贴图
    public class MagneticHeavenBlade : JuexueItem
    {
        public override JuexueID JuexueId => JuexueID.Active_MagneticHeavenBlade;
        public override int QiCost => 50;
        public override int SpecialCooldownTicks => 60 * 10; // 10 秒专属冷却（示例）

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("绝学·磁场天刀");
            // Tooltip.SetDefault("主动（50气）：短暂吸引附近敌怪到鼠标附近，并从天而降刀雨。");
        }

        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            Vector2 mouse = Main.MouseWorld;
            SoundEngine.PlaySound(SoundID.Item8, mouse);

            // 吸引（瞬时速度脉冲）
            float radius = 600f;
            for (int i = 0; i < 12; i++)
            {
                Vector2 spawn = mouse.ToScreenPosition() + Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 12) * radius;
                Dust dust = Dust.NewDustPerfect(spawn, DustID.GemDiamond, null, 0, default, 1.5f);
                dust.noGravity = true;
            }
            foreach (var npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && Vector2.Distance(npc.Center, mouse) <= radius)
                {
                    Vector2 dir = (mouse - npc.Center).SafeNormalize(Vector2.Zero);
                    npc.velocity += dir * 12f; // 轻量吸引
                }
            }

            // 刀雨：用原版 FallingStar/StarWrath 占位
            int count = 12;
            for (int i = 0; i < count; i++)
            {
                Vector2 spawn = mouse + new Vector2(Main.rand.NextFloat(-300, 300), -600f + Main.rand.NextFloat(-60, 60));
                Vector2 v = (mouse - spawn).SafeNormalize(Vector2.UnitY) * 18f;
                int projType = ProjectileID.StarWrath;
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), spawn, v, projType, 60, 2f, player.whoAmI);
            }
            return true;
        }
    }
}
