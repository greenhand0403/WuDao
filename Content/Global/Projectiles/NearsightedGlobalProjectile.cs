using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;

namespace WuDao.Content.Global.Projectiles
{
    // 近视眼镜：用于给所有射弹记录“发射源NPC”的全局组件
    public class NearsightedGlobalProjectile : GlobalProjectile
    {
        public int SourceNPC = -1;   // 记录发射者NPC的 whoAmI

        public override bool InstancePerEntity => true;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // 1.4.4 的原版/模组通常都会用带 Entity 的 source 构造器
            if (source is EntitySource_Parent esp && esp.Entity is NPC npc)
                SourceNPC = npc.whoAmI;
        }
    }
}