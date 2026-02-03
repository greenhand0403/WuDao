using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items
{
    /// <summary>
    /// 异果：增加气力上限10，只能使用5次
    /// </summary>
    public class PassionFruit : ModItem
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
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 1, 50);
            Item.maxStack = 5;
        }
        public override bool CanUseItem(Player player)
        {
            var qi = player.GetModPlayer<QiPlayer>();
            return qi.Used_PassionFruit < 5;
        }
        public override bool? UseItem(Player player)
        {
            var qi = player.GetModPlayer<QiPlayer>();
            qi.Used_PassionFruit++;
            qi.QiMaxFromItems += 10;
            Main.NewText("你感到内息更稳（气力上限 +10）。", Microsoft.Xna.Framework.Color.LightSeaGreen);
            return true;
        }
    }
}
