using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace WuDao.Content.DamageClasses
{
    public class SupremeDamageClass : DamageClass
    {
        public override StatInheritanceData GetModifierInheritance(DamageClass dc)
        {
            var data = StatInheritanceData.None;

            // 超能 = 魔法(50%) + 召唤(50%)
            if (dc == DamageClass.Magic || dc == DamageClass.Summon)
            {
                data.damageInheritance = 0.5f;
                // 召唤本体不走暴击，通常只从魔法继承暴击更合理：
                if (dc == DamageClass.Magic)
                    data.critChanceInheritance = 0.5f;

                data.knockbackInheritance = 0.5f;
                data.armorPenInheritance = 0.5f;
            }

            // 如果你还想让“鞭子攻速（SummonMeleeSpeed）”参与，可以额外对该类给 attackSpeedInheritance
            // if (dc == DamageClass.SummonMeleeSpeed) data.attackSpeedInheritance = 0.5f;

            return data;
        }

        public override bool GetEffectInheritance(DamageClass dc)
            => dc == DamageClass.Magic || dc == DamageClass.Summon;

        public override bool UseStandardCritCalcs => true;
    }
}
