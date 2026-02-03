using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Global.Projectiles
{
    // 模仿者：用于标记“是否来自模仿者”的弹体
    public class MimickerGlobalProjectile : GlobalProjectile
    {
        public bool fromMimicker;
        public override bool InstancePerEntity => true;
    }
}