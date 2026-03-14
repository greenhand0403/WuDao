using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    public class BeetleArrow : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;

            Item.accessory = true;

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<BeetleArrowPlayer>().beetleArrow = true;

            player.GetDamage(DamageClass.Melee) += 0.10f;
            player.GetDamage(DamageClass.Summon) += 0.10f;
        }
    }
}