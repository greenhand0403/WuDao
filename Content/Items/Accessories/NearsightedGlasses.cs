using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    public class NearsightedGlasses : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_1742";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 3, 0, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var gp = player.GetModPlayer<NearsightedPlayer>();
            gp.Nearsighted = true;
            gp.ShowRangeRings = !hideVisual;
        }
    }

}
