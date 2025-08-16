using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace WuDao.Content.DamageClasses
{
    public class ChiEnergyDamageClass : DamageClass
    {
        public override StatInheritanceData GetModifierInheritance(DamageClass dc)
        {
            var data = StatInheritanceData.None;

            // 真气 = 近战(50%) + 远程(50%)
            if (dc == DamageClass.Melee || dc == DamageClass.Ranged)
            {
                data.damageInheritance = 0.5f;        // 伤害%
                data.critChanceInheritance = 0.5f;    // 暴击%（不想继承就删掉）
                data.knockbackInheritance = 0.5f;     // 击退（可选）
                data.attackSpeedInheritance = 0.5f;   // 攻速（可选）
                data.armorPenInheritance = 0.5f; // 护甲穿透（可选）
                                                         // 还有 flatDamageInheritance / scalingArmorPenetration 等字段，可按需使用
            }
            return data;
        }

        public override bool GetEffectInheritance(DamageClass dc)
            => dc == DamageClass.Melee || dc == DamageClass.Ranged; // 让近战/远程限定效果也影响真气

        public override bool UseStandardCritCalcs => true;
    }
}
