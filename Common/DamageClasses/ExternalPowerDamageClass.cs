using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace WuDao.Common.DamageClasses
{
    public class ExternalPowerDamageClass : DamageClass
    {
        public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
        {
            // TODO: 叠加了4%的暴击？我设置了4%飞蝗石，结果显示8%
            if (damageClass == DamageClass.Generic)
                return StatInheritanceData.Full;
            if (damageClass == DamageClass.Melee)
            {
                return new StatInheritanceData(
                    damageInheritance: 0.5f,
                    critChanceInheritance: 0f,
                    attackSpeedInheritance: 0f,
                    armorPenInheritance: 0f,
                    knockbackInheritance: 0f
                );
            }
            return StatInheritanceData.None;
        }

        public override bool GetEffectInheritance(DamageClass damageClass)
        {
            return damageClass == DamageClass.Melee;
        }

        public override bool UseStandardCritCalcs => true;
    }
}
