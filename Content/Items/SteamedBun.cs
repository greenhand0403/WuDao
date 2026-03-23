using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Systems;

namespace WuDao.Content.Items
{
    /// <summary>
    /// 馒头：召唤流浪乞丐 NPC
    /// </summary>
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
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(0, 0, 50);
        }

        public override bool? UseItem(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return true;

            // 50% 的几率召唤流浪乞丐
            if (Main.rand.NextBool(2))
            {
                return false;
            }

            BeggarSystem.SpawnBeggarNear(player);

            if (Main.netMode != NetmodeID.Server)
                Main.NewText(Language.GetTextValue("Mods.WuDao.Items.SteamedBun.Messages"), Color.LightGreen);

            return true;
        }
    }
}
