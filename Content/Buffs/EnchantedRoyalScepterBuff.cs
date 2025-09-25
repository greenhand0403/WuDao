using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Buffs
{
    // 标准召唤 Buff：保证随从存活
    public class EnchantedRoyalScepterBuff : ModBuff
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.RoyalScepter}";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("附魔皇家权杖");
            // Description.SetDefault("一把皇家权杖在你头顶守护你");
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.debuff[Type] = false; // 允许右键移除
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 维持一个可见时长（即便不显示时间），避免自然掉光
            player.buffTime[buffIndex] = 18000;

            // 如果没有在场，就由本地玩家生成 1 把
            if (player.whoAmI == Main.myPlayer &&
                player.ownedProjectileCounts[ModContent.ProjectileType<RoyalScepterMinion>()] <= 0)
            {
                var pos = player.Center - new Microsoft.Xna.Framework.Vector2(0f, 50f);
                Terraria.Projectile.NewProjectile(
                    player.GetSource_Buff(buffIndex),
                    pos, Microsoft.Xna.Framework.Vector2.Zero,
                    ModContent.ProjectileType<RoyalScepterMinion>(),
                    player.GetWeaponDamage(player.HeldItem),
                    player.GetWeaponKnockback(player.HeldItem),
                    player.whoAmI);
            }
        }

        // 可选：显式支持右键移除
        public override bool RightClick(int buffIndex) => true;
    }
}
