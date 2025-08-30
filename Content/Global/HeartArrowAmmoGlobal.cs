using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Projectiles.Ranged;
using WuDao.Content.Items.Weapons.Ranged;
using WuDao.Content.Items.Ammo;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Global
{
    public class HeartArrowAmmoGlobal : GlobalItem
    {
        public override bool InstancePerEntity => false;

        public override bool CanConsumeAmmo(Item weapon, Item ammo, Player player)
        {
            var mp = player.GetModPlayer<HeartStuffPlayer>();

            // 仅对“心灵宝石 + 心箭弹药”生效：20%不耗弹
            if (mp.SoulGemEquipped && ammo.type == ModContent.ItemType<HeartArrow>())
            {
                if (Main.rand.NextFloat() < 0.20f)
                    return false;
            }
            return base.CanConsumeAmmo(weapon, ammo, player);
        }

        public override void OnConsumeAmmo(Item weapon, Item ammo, Player player)
        {
            // 使用“心箭弹药”时的生命消耗（所有弓）
            if (ammo.type == ModContent.ItemType<HeartArrow>())
            {
                var mp = player.GetModPlayer<HeartStuffPlayer>();

                // 如果武器就是丘比特弓，则此处应按照“丘比特弓=1生命”的规则；
                // 但丘比特弓在 Shoot 已经扣过一次，这里为了通用（非丘比特弓的情况）我们仅在不是丘比特弓时处理。
                bool usingCupid = weapon.type == ModContent.ItemType<CupidBow>();
                if (!usingCupid)
                {
                    int lifeCost = mp.GetHeartArrowLifeCost(false);
                    if (lifeCost > 0 && player.statLife > lifeCost)
                    {
                        player.statLife -= lifeCost;
                        player.HealEffect(-lifeCost, broadcast: true);
                    }
                }
            }
        }

        public override void ModifyShootStats(
        Item item, Player player,
        ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // ✅ 只在“最终射弹 == 心箭”时才前推
            if (type != ModContent.ProjectileType<HeartArrowProj>())
                return;

            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);

            // 只在“与玩家距离足够”且“可通行”时前推，避免把弹丸塞进头顶平台/天花板
            float cursorDist = Vector2.Distance(player.MountedCenter, Main.MouseWorld);
            if (cursorDist >= 24f) // 准星太近就不前推
            {
                Vector2 muzzle = dir * 40f; // 32~56 视手感
                if (Collision.CanHit(position, 0, 0, position + muzzle, 0, 0))
                    position += muzzle;

                // 双发/多发互相重叠的轻微偏移（只有在可通时才加）
                Vector2 micro = dir * 6f;
                if (Collision.CanHit(position, 0, 0, position + micro, 0, 0))
                    position += micro;
            }
        }
    }

    public class HeartPickupGlobal : GlobalItem
    {
        public override bool OnPickup(Item item, Player player)
        {
            if (item.type == ItemID.Heart)
            {
                var mp = player.GetModPlayer<HeartStuffPlayer>();
                if (mp.SoulGemEquipped && player.statLife < player.statLifeMax2)
                {
                    player.statLife = System.Math.Min(player.statLife + 2, player.statLifeMax2);
                    player.HealEffect(2, true);
                }
            }
            return base.OnPickup(item, player);
        }
    }
}