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
        public const float MULT = 0.8f;
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
                StatEffect.DamageAdd(-(1-MULT)),
                StatEffect.AttackSpeedAdd(-(1-MULT)),
                StatEffect.MoveSpeed(-(1-MULT)),
                
                StatEffect.DefenseMultiplier(MULT)
            ));
        }
        
    }
}
