using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Systems;

namespace WuDao.Content.Items
{
    public class SteamedBun : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 10;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.UseSound = SoundID.Item2;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0, 0, 50);
        }

        public override bool? UseItem(Player player)
        {
            BeggarSystem.SpawnBeggarNear(player);
            Main.NewText("你招来了流浪乞丐。", Microsoft.Xna.Framework.Color.LightGreen);
            return true;
        }
    }
}
