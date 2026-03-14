using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Mounts;

namespace WuDao.Content.Items
{
    // 月亮船坐骑
    public class LunarShipMountItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.mountType = ModContent.MountType<LunarShip>();
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.buyPrice(platinum: 1);
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            // 使用时自动上坐骑
            if (player.whoAmI == Main.myPlayer && player.mount.Type != Item.mountType)
            {
                player.mount.SetMount(Item.mountType, player);
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddCondition(Condition.BloodMoon)
                .AddIngredient(ItemID.PirateShipMountItem)
                .AddTile(TileID.LihzahrdFurnace)
                .Register();
        }
    }
}
