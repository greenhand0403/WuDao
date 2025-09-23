using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;
using WuDao.Content.Projectiles;

namespace WuDao.Content.Items.Pets
{
    public class DiscoBallRemote : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.noMelee = true;
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.LightRed;
            Item.shoot = ModContent.ProjectileType<DiscoBallPetProj>();
            Item.buffType = ModContent.BuffType<DiscoBallPetBuff>();
            Item.UseSound = SoundID.Item44; // 召唤宠物常用音效
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 360);
            }
        }
        public override void AddRecipes()
        {
            // 先定义一个 RecipeGroup，把三种仙灵都算作同一类材料
            RecipeGroup fairyGroup = new RecipeGroup(() => "任意仙灵",
                ItemID.FairyCritterBlue,
                ItemID.FairyCritterGreen,
                ItemID.FairyCritterPink);

            // 注册 RecipeGroup，给它一个 key
            RecipeGroup.RegisterGroup("WuDao:FairyGroup", fairyGroup);

            // 然后在配方里使用这个组
            Recipe recipe = CreateRecipe()
                .AddRecipeGroup("WuDao:FairyGroup", 3) // 任意仙灵 3 个
                .AddIngredient(ItemID.HallowedBar, 3)   // 神圣锭 3 个
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
