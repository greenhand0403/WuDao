using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items.Weapons.Magic;

namespace WuDao.Content.Players
{
    // 魔法号角 随机射弹辅助类
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

                if (hornTimer >= 60 * 2) // 2秒
                {
                    hornTimer = 0;

                    // 朝鼠标方向发射
                    Vector2 dir = (Main.MouseWorld - Player.MountedCenter)
                        .SafeNormalize(new Vector2(Player.direction, 0f));
                    float speed = 14f;
                    Vector2 vel = dir * speed;

                    // 召唤冲锋“蜥蜴”弹
                    int idx = Projectile.NewProjectile(
                        new EntitySource_Misc("MagicHornCharge"),
                        Player.MountedCenter,
                        vel,
                        ModContent.ProjectileType<BasiliskChargeProj>(),
                        Player.GetWeaponDamage(Player.HeldItem),
                        Player.HeldItem.knockBack,
                        Player.whoAmI
                    );
                }
            }
        }
    }
}
