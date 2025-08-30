using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    public class GuixinAccessory : ModItem
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("归心似箭");
        //     Tooltip.SetDefault("回旋镖返回途中飞行速度是抛出时的两倍");
        // }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(silver: 80);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<BoomerangAccessoryPlayer>().Guixin = true;
        }
    }
}