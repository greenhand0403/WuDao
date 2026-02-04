using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    // 心灵宝石，增强心箭的能力
    public class SoulGem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<HeartStuffPlayer>().SoulGemEquipped = true;
        }
        public override void AddRecipes()
        {
            Recipe.Create(ModContent.ItemType<SoulGem>())
                .AddIngredient(ItemID.ManaCrystal, 5)
                .AddIngredient(ItemID.LifeCrystal, 5)
                .AddIngredient(ItemID.Diamond, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}