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
using WuDao.Content.Projectiles.Melee;
using System;

namespace WuDao.Content.Juexue.Active
{
    public class MagneticHeavenBlade : JuexueItem
    {
        public override int QiCost => 50;
        public override int SpecialCooldownTicks => 60 * 60; // 1 分钟冷却
        public const int MagneticHeavenBladeFrameIndex = 13;
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            Vector2 mouse = Main.MouseWorld;
            SoundEngine.PlaySound(SoundID.Item8, mouse);

            // —— 1) 生成“场域”持续吸引（0.5秒）—— //
            float radius = 600f;
            var src = player.GetSource_ItemUse(Item);
            int field = Projectile.NewProjectile(src, mouse, Vector2.Zero, ModContent.ProjectileType<MagneticHeavenBladeField>(), 0, 0f, player.whoAmI, radius);
            if (field >= 0 && Main.projectile[field].active)
            {
                // 生成一圈简易的闪光/尘用于演出
                for (int i = 0; i < 24; i++)
                {
                    Vector2 pos = mouse + Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 24f) * radius;
                    var d = Dust.NewDustPerfect(pos, DustID.GemDiamond, (mouse - pos).SafeNormalize(Vector2.Zero) * 3f, 0, default, 1.4f);
                    d.noGravity = true;
                }
            }
            // 生成磁场天刀专属射弹
            // 在中心上方 [-320, 320] 的随机水平偏移，垂直出生在中心上方 600~720 像素
            Vector2 spawn = mouse + new Vector2(Main.rand.NextFloat(-320f, 320f), -Main.rand.NextFloat(600f, 720f));

            // 初速度朝向中心
            Vector2 v = (mouse - spawn).SafeNormalize(Vector2.UnitY) * 22f;
            int damage = (int)(player.GetWeaponDamage(Item) * 2.2f) + 20 * Helpers.BossProgressPower.GetUniqueBossCount();
            int p = Projectile.NewProjectile(src, spawn, v, ModContent.ProjectileType<MagneticHeavenBladeProj>(), Math.Max(damage, 100), 3f, player.whoAmI, mouse.X, mouse.Y);

            if (p >= 0)
            {
                var proj = Main.projectile[p];
                proj.netUpdate = true;
            }

            if (!Main.dedServ)
            {
                // 触发 2 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
                qi.TriggerJuexueGhost(MagneticHeavenBladeFrameIndex, durationTick: 120, scale: 1.1f, offset: new Vector2(0, -16));
            }
            return true;
        }
    }
}
