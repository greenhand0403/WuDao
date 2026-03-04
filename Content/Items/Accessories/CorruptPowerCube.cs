using Terraria;
using Terraria.ID;
using WuDao.Common;
using System.Collections.Generic;

namespace WuDao.Content.Items.Accessories
{
    /// <summary>
    /// 腐化立方，增加7点防御，4%减伤2%移速，腐化环境+4%伤害
    /// </summary>
    public class CorruptPowerCube : BuffItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 5);
            Item.defense = 7;
        }
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            // 4%减伤2%移速，腐化环境+4%伤害
            rules.Add(new StatRule(BuffConditions.Always,
                StatEffect.EnduranceAdd(0.04f),
                StatEffect.MoveSpeed(0.02f)
            ));
            rules.Add(new StatRule(BuffConditions.InCorrupt,
                StatEffect.DamageAdd(0.04f)
            ));
        }
    }
}
