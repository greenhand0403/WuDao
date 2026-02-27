// Buffs/FlyingToiletBuff.cs
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Mounts;

namespace WuDao.Content.Buffs
{
    public class LunarShipBuff : ModBuff
    {
        public override string Texture => $"WuDao/Content/Items/LunarShipMountItem";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; // 坐骑 buff 无限
            Main.buffNoSave[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            // 每 tick 重置 buffTime
            player.buffTime[buffIndex] = 10;

            if (player.mount.Type != ModContent.MountType<LunarShip>())
            {
                player.mount.SetMount(ModContent.MountType<LunarShip>(), player);
            }
        }
    }
}
