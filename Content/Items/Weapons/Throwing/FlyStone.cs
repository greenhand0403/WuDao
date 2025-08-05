// FlyStone.cs - 飞蚊石武器
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Items.Weapons.Throwing
{
    public class FlyStone : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 8;
            Item.DamageType = DamageClass.Throwing;
            Item.width = 12;
            Item.height = 12;
            Item.useTime = 12;
            Item.useAnimation = 12;

            Item.crit = 0;
            Item.value = Item.sellPrice(silver: 1);
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.Shoot; // 正确用法
            Item.maxStack = 9999;

            Item.consumable = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.Throwing.FlyStoneProjectile>();
            Item.shootSpeed = 10f;
            Item.UseSound = SoundID.Item1;
        }

        public override void AddRecipes()
        {
            CreateRecipe(30)
                .AddIngredient(ItemID.StoneBlock, 10)
                .AddTile(TileID.Furnaces)
                .Register();
        }
    }
}