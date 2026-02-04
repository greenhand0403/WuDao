using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Ranged;

namespace WuDao.Content.Items.Ammo
{
    /// <summary>
    /// 心箭：射出时消耗2点生命；命中恢复1点生命；击杀必掉治疗红心；带自动追踪
    /// </summary>
    public class HeartArrow : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(silver: 1);
            Item.ammo = AmmoID.Arrow;
            Item.shoot = ModContent.ProjectileType<HeartArrowProj>();
            Item.shootSpeed = 1f;
            Item.damage = 8;
            Item.knockBack = 1f;
            Item.DamageType = DamageClass.Ranged;
        }

        public override void AddRecipes()
        {
            // 心箭：木箭 + 红心 ；工作台
            CreateRecipe(100)
                .AddIngredient(ItemID.WoodenArrow, 100)
                .AddIngredient(ItemID.Heart, 1) // 也可换成生命水晶/凝胶等
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}