using Terraria.ModLoader;

namespace WuDao.Content.DamageClasses
{
    public class SupremeDamageClass : DamageClass
    {
        public override StatInheritanceData GetModifierInheritance(DamageClass dc)
        {
            var data = StatInheritanceData.None;

            // 超能伤害类：魔法(80%) + 召唤(80%)
            if (dc == DamageClass.Magic)
            {
                return new StatInheritanceData(
                    damageInheritance: 0.8f,
                    critChanceInheritance: 0.8f,
                    attackSpeedInheritance: 0.8f,
                    armorPenInheritance: 0.8f
                );
            }

            if (dc == DamageClass.Summon)
            {
                return new StatInheritanceData(
                    damageInheritance: 0.8f,
                    critChanceInheritance: 0.8f
                );
            }
            return data;
        }

        public override bool GetEffectInheritance(DamageClass dc)
            => dc == DamageClass.Magic || dc == DamageClass.Summon; // 让魔法/召唤限定效果也影响超能伤害类

        public override bool UseStandardCritCalcs => true;
    }
}
