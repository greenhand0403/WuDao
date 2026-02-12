using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Buffs
{
    public class ThousandGhostBannerBuff : ModBuff
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.GhostBanner}";

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; // 这个 buff 不显示持续时间
            Main.buffNoSave[Type] = true; // 这个 buff 不保存
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // If the minions exist reset the buff time, otherwise remove the buff from the player
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ThousandGhostMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}