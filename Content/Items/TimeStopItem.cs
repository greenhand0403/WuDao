using Terraria;
using Terraria.ID;
using WuDao.Content.Systems;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Items
{
    public class TimeStopItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 34;
            Item.useAnimation = 34;
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item4;
        }

        public override bool? UseItem(Player player)
        {
            if (!TimeStopSystem.IsFrozen)
            {
                TimeStopSystem.StartFreeze(180);
                CombatText.NewText(player.getRect(), Color.Cyan, "Time... STOP!");
            }
            return true;
        }

    }
}