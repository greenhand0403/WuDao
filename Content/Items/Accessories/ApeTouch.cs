using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    public class ApeTouch : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_1";

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ApeTouchPlayer>().ApeTouch = true;
        }
    }
}
