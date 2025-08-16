using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Items.Weapons.Magic
{
    public class MagicHornPlayer : ModPlayer
    {
        private int hornTimer;

        public override void ResetEffects()
        {
            // 非使用状态就慢慢归零
            if (!Player.controlUseItem || Player.HeldItem.type != ModContent.ItemType<MagicHorn>())
                hornTimer = 0;
        }

        public override void PreUpdate()
        {
            if (Player.HeldItem.type == ModContent.ItemType<MagicHorn>() &&
                Player.controlUseItem && Player.itemAnimation > 0)
            {
                hornTimer++;

                if (hornTimer >= 60 * 3) // 3秒
                {
                    hornTimer = 0;

                    // 计算一个朝向最近敌怪的速度
                    Vector2 dir = Vector2.UnitX * Player.direction;
                    float speed = 14f;

                    NPC target = FindNearestEnemy(Player.Center, 900f);
                    if (target != null)
                        dir = (target.Center - Player.Center).SafeNormalize(dir);

                    Vector2 vel = dir * speed;

                    // 召唤冲锋“蜥蜴”弹
                    Projectile.NewProjectile(
                        new EntitySource_Misc("MagicHornCharge"),
                        Player.Center, vel,
                        ModContent.ProjectileType<BasiliskChargeProj>(),
                        Player.GetWeaponDamage(Player.HeldItem),
                        Player.HeldItem.knockBack,
                        Player.whoAmI
                    );
                }
            }
        }

        private NPC FindNearestEnemy(Vector2 from, float maxDist)
        {
            NPC best = null;
            float bestD = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.active && !n.friendly && !n.dontTakeDamage && n.CanBeChasedBy())
                {
                    float d = Vector2.Distance(from, n.Center);
                    if (d < bestD && Collision.CanHitLine(from, 1, 1, n.Center, 1, 1))
                    {
                        best = n; bestD = d;
                    }
                }
            }
            return best;
        }
    }
}
