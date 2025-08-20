// TODO: FlyStone 飞蝗石 可以提取出基类，方便我以后做可投掷的宝石
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Items.Weapons.Throwing
{
    public class FlyStone : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Shoot; // 正确用法
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<FlyStoneProjectile>();
            Item.damage = 6;
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.UseSound = SoundID.Item1;
            Item.useAnimation = 12;
            Item.useTime = 12;
            Item.autoReuse = true;
            
            Item.value = Item.sellPrice(silver: 1);
            Item.rare = ItemRarityID.Green;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.knockBack = 1;
            Item.DamageType = DamageClass.Throwing;
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