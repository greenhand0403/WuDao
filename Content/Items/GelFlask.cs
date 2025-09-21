using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using WuDao.Content.Buffs;

namespace WuDao.Content.Items
{
    class GelFlask : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.FlaskofIchor}";
        public override void SetDefaults()
        {
            Item.UseSound = SoundID.Item3;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useTurn = true;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 14;
            Item.height = 24;
            Item.buffType = ModContent.BuffType<GelFlaskBuff>();
            Item.buffTime = Item.flaskTime;
            Item.value = Item.sellPrice(0, 0, 5);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater)
                .AddIngredient(ItemID.Gel)
                // .AddIngredient<ExampleItem>(2)
                .AddTile(TileID.ImbuingStation)
                .Register();
        }
    }
}