using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    public class YueshengAccessory : ModItem
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("跃升");
        //     Tooltip.SetDefault("回旋镖可以穿墙且对敌无限穿透");
        // }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(gold: 1);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<BoomerangAccessoryPlayer>().Yuesheng = true;
        }
    }
}