using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Global;

namespace WuDao.Content.Players
{
    public class TimeStopPlayer : ModPlayer
    {
        public override bool CanUseItem(Item item)
        {
            if (TimeStopSystem.IsFrozen)
            {
                // 禁止近战、远程、魔法、召唤攻击
                if (item.damage > 0 || item.shoot > ProjectileID.None)
                    return false;
            }
            return true;
        }

        public override void PreUpdate()
        {
            TimeStopSystem.Update(); // 每帧刷新冻结计时器
        }
    }
}
