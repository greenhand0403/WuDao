using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using WuDao.Content.Buffs;

namespace WuDao.Content.Global.NPCs
{
    class GelGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        // 在每帧生效：如果NPC被凝胶覆盖，则削弱其速度
        public override void AI(NPC npc)
        {
            if (npc.HasBuff(ModContent.BuffType<GelFlaskDebuff>()))
            {
                // 轻微持续减速：逐步降低速度（这在大多数AI下都会有效）
                npc.velocity *= 0.92f;
                // 为了防止被强制复位的速度错误，可以降低某些运动参数（保守做法）
                // 注意：不同NPC的AI会覆盖速度，这只是一个可靠且简单的减速手段
            }
        }


        // OnFire等DoT通常通过lifeRegen实现，这里放大那些DoT造成的生命回复扣减
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!npc.HasBuff(ModContent.BuffType<GelFlaskDebuff>()))
                return;


            // 如果NPC处于"着火"或"燃烧"类debuff，我们放大其lifeRegen造成的负值
            // 检测常见燃烧debuff：On Fire! 和 Burning（如果存在）


            bool onFire = npc.HasBuff(BuffID.OnFire);
            bool burning = npc.HasBuff(BuffID.Burning) || npc.HasBuff(BuffID.CursedInferno); // 兼容性检测


            if (onFire || burning)
            {
                // 传统上，lifeRegen 负值表示每秒造成的伤害（负数的一半/2），这里我们降低lifeRegen更多以放大DoT
                // 这里对damage（传出参数）进行修改：增加额外伤害的保守值
                damage += 4; // 每tick额外 +4 点（按照框架会转化为每秒若干）


                // 另外，也放大npc.lifeRegenDecrease（更直接控制DoT的生命恢复）
                // 在一些TL版本中，lifeRegen 被表示为 npc.lifeRegen; 我们谨慎修改damage参数来兼容更多版本。
            }
        }
    }
}
