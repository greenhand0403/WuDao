// 文件：YourMod/Common/BossOwnerGlobalProjectile.cs
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace WuDao.Content.Global.Projectiles
{
    // 用于败笔记录boss
    public class BossOwnerGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        // 记录这个弹幕来自哪个 NPC（whoAmI）
        public int OwnerNPC = -1;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // 常见：NPC AI 用 EntitySource_Parent/EntitySource_FromAI 生成弹幕
            // 我们尽可能从 source 里取到 NPC
            if (source is EntitySource_Parent p && p.Entity is NPC npc1)
            {
                OwnerNPC = npc1.whoAmI;
            }
            // else if (source is EntitySource_FromAI a && a.Entity is NPC npc2)
            // {
            //     OwnerNPC = npc2.whoAmI;
            // }
            // 其他少见情况可以继续扩：例如 EntitySource_Parent? / EntitySource_Misc 等
            // 默认为 -1：未知来源
        }
    }
}
