using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items
{
    public class ReiShi: ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item4;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(0, 1);
        }
        public override bool CanUseItem(Player player)
        {
            var qi = player.GetModPlayer<QiPlayer>();
            return qi.Used_LingZhi < 1;
        }
        public override bool? UseItem(Player player)
        {
            var qi = player.GetModPlayer<QiPlayer>();
            qi.Used_LingZhi++;
            qi.QiMaxFromItems += 50;
            Main.NewText("你感到丹田充盈（气力上限 +50）。", Microsoft.Xna.Framework.Color.SkyBlue);
            return true;
        }
    }
}
