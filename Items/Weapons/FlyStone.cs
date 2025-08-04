// FlyStone.cs - 飞蚊石武器
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WuDao.Common.DamageClasses; // 确保你已经定义 ExternalPowerDamageClass

namespace WuDao.Items.Weapons
{
    public class FlyStone : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 8;
            Item.DamageType = ModContent.GetInstance<ExternalPowerDamageClass>();
            Item.width = 12;
            Item.height = 12;
            Item.useTime = 12;
            Item.useAnimation = 12;
            
            Item.crit = 4;
            Item.value = Item.sellPrice(silver: 1);
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.Shoot; // 正确用法
            Item.maxStack = 9999;

            Item.consumable = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.FlyStoneProjectile>();
            Item.shootSpeed = 10f;
            Item.UseSound = SoundID.Item1;
            // Item.ammo = Item.type; // 自我类型
        }

        public override void AddRecipes()
        {
            CreateRecipe(30)
                .AddIngredient(ItemID.StoneBlock, 10)
                .AddTile(TileID.Furnaces)
                .Register();
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            foreach (var line in tooltips)
            {
                if (line.Name == "Damage" && line.Mod == "Terraria")
                {
                    line.Text = line.Text.Replace("damage", "外功伤害");
                }
            }
        }
    }
}