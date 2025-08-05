using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace WuDao.Content.DamageClasses
{
    public class ChiEnergyDamageClass : DamageClass
    {
        public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
        {
            StatInheritanceData statInheritanceData = StatInheritanceData.None;
            // TODO： 未测试能否继承近战伤害和远程伤害
            if (damageClass == DamageClass.Melee)
            {
                statInheritanceData.damageInheritance = 0.5f;
            }
            if (damageClass == DamageClass.Ranged)
            {
                statInheritanceData.damageInheritance = 0.5f;
            }
            return statInheritanceData;
        }

        public override bool GetEffectInheritance(DamageClass damageClass)
        {
            return damageClass == DamageClass.Magic || damageClass == DamageClass.Summon;
        }

        public override bool UseStandardCritCalcs => true;
    }
}
