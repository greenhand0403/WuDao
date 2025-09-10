using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Global.NPCs;

namespace WuDao.Content.Items
{
    public class FoodGodSummonItem : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.ApplePie}";
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = Item.useAnimation = 30;
            Item.consumable = true; Item.maxStack = 20;
            Item.rare = ItemRarityID.Green;
        }

        public override bool CanUseItem(Player player)
            => !NPC.AnyNPCs(ModContent.NPCType<FoodGodBoss>());

        public override bool? UseItem(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var pos = player.Center + new Microsoft.Xna.Framework.Vector2(0, -600);
                NPC.NewNPC(null, (int)pos.X, (int)pos.Y, ModContent.NPCType<FoodGodBoss>());
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBar, 5)     // “金怡口”和“碗”简化用金锭+碗
                .AddIngredient(ItemID.Bowl, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}