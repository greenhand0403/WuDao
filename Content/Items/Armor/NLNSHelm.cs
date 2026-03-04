using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class NLNSHelm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.vanity = true;          // 纯外观
            Item.defense = 0;            // 不加防御（可省略，默认0）
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 50);
        }
    }
}