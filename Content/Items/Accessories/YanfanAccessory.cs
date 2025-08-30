using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    public class YanfanAccessory : ModItem
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("燕返");
        //     Tooltip.SetDefault("回旋镖在返回途中造成的伤害翻倍");
        // }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(silver: 50);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<BoomerangAccessoryPlayer>().Yanfan = true;
        }
    }
}