using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{

    public class SoulGem : ModItem
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("心灵宝石");
        //     Tooltip.SetDefault(
        //         "使用心箭不消耗生命\n" +
        //         "20% 概率不消耗心箭弹药\n" +
        //         "心箭命中回复2生命\n" +
        //         "你拾取的治疗红心额外恢复2生命\n" +
        //         "心箭自动追踪半径提升至7格"
        //     );
        // }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<HeartStuffPlayer>().SoulGemEquipped = true;
        }
        public override void AddRecipes()
        {
            // 心灵宝石：魔法宝石+生命水晶
            Recipe.Create(ModContent.ItemType<SoulGem>())
                .AddIngredient(ItemID.ManaCrystal, 2)
                .AddIngredient(ItemID.LifeCrystal, 1)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}