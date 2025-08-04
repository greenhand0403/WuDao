using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace WuDao.Common.DamageClasses
{
    public class InternalPowerDamageClass : DamageClass
    {
        public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
        {
            StatInheritanceData statInheritanceData = StatInheritanceData.None;
            if (damageClass == DamageClass.Magic)
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
