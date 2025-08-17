using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Buffs; // 你的框架命名空间
using System.Collections.Generic;
using WuDao.Content.Global.NPCs;
namespace WuDao.Content.Items.Accessories
{
    // TODO: 贴图置换 压制力场
    public class DevolutionCharm : BuffItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.OldShoe}";
        private const float MULT = 0.5f;
        public override void SetDefaults()
        {
            Item.width = 28; Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.buyPrice(0, 5, 0, 0);
        }

        // 玩家自身“全面退化”：所有这些都×0.8
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            rules.Add(new StatRule(BuffConditions.Always,
                // 生命/法力 上限 & 再生
                StatEffect.MaxLifeMultiplier(MULT),
                StatEffect.LifeRegenMultiplier(MULT),
                StatEffect.MaxManaMultiplier(MULT),
                StatEffect.ManaRegenMultiplier(MULT),

                // 攻速/伤害/移速/防御
                StatEffect.AttackSpeedMultiplier(MULT),
                // StatEffect.DamageMultiplier(MULT),

                StatEffect.MoveSpeed(mult: MULT),      // 移动速度 ×0.8
                StatEffect.DefenseMultiplier(MULT)     // 防御 ×0.8
            ));
        }

        // 这里不用写敌怪效果，见第三部分（GlobalNPC/GlobalProjectile）
    }
    public class DevolutionDamageScaleGlobal : GlobalItem
    {
        public const float POSITIVE_SCALE = 0.8f; // 80%

        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            // 仅在佩戴了“全面退化”饰品时生效
            if (!player.GetModPlayer<DevolutionPlayer>().HasDevolutionAura)
                return;

            // 关键：把当前累计好的伤害 StatModifier 朝 1.0F 缩放到 80%
            damage = damage.Scale(POSITIVE_SCALE);
            // 让负面也变小（-10% 仍保持 -10%），只想削弱正增的写法
            // float add = damage.Additive - 1f;
            // if (add > 0) damage.Additive = 1f + add * 0.8f;
        }
    }
}
