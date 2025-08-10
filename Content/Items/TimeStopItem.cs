using Terraria;
using Terraria.ID;
using WuDao.Common;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Items
{
    // TODO: 并不完美的冻结时间
    public class TimeStopItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item4;
        }

        public override bool? UseItem(Player player)
        {
            if (!TimeStopSystem.IsFrozen)
            {
                TimeStopSystem.StartFreeze(300);
                CombatText.NewText(player.getRect(), Color.Cyan, "Time... STOP!");
            }
            return true;
        }

    }
}