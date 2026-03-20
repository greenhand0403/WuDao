using Terraria.ModLoader;

namespace WuDao.Content.DamageClasses
{
    // 真气伤害类
    public class ChiEnergyDamageClass : DamageClass
    {
        public override StatInheritanceData GetModifierInheritance(DamageClass dc)
        {
            var data = StatInheritanceData.None;

            // 真气 = 近战(80%) + 远程(80%)
            if (dc == DamageClass.Melee)
            {
                return new StatInheritanceData(
                    damageInheritance: 0.8f,
                    critChanceInheritance: 0.8f,
                    attackSpeedInheritance: 0.8f,
                    armorPenInheritance: 0.8f,
                    knockbackInheritance: 0.8f
                );
            }

            if (dc == DamageClass.Ranged)
            {
                return new StatInheritanceData(
                    damageInheritance: 0.8f,
                    critChanceInheritance: 0.8f,
                    attackSpeedInheritance: 0.8f,
                    armorPenInheritance: 0.8f,
                    knockbackInheritance: 0.8f
                );
            }
            return data;
        }

        public override bool GetEffectInheritance(DamageClass dc)
            => dc == DamageClass.Melee || dc == DamageClass.Ranged; // 让近战/远程限定效果也影响真气

        public override bool UseStandardCritCalcs => true;
    }
}
