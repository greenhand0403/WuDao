using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Players
{
    public class SeedMinigunPlayer : ModPlayer
    {
        // 当前还可以发射多少发
        public int seedMinigunShots;

        // 距离下一次“补充5~8发”还剩多少tick
        public int seedMinigunRefillCooldown;

        public override void PreUpdate()
        {
            if (seedMinigunRefillCooldown > 0)
                seedMinigunRefillCooldown--;
        }

        public void TryRefillSeedMinigunAmmo()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (seedMinigunShots <= 0 && seedMinigunRefillCooldown <= 0)
            {
                seedMinigunShots = Main.rand.Next(5, 9); // 5~8发
                seedMinigunRefillCooldown = 60;          // 1秒后才能再次补充
            }
        }
    }
}