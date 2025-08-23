using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Items.Weapons.Melee
{
    // TODO: 近战刀光武器，完全不能用，重做 崇阳铁剑
    public class SteelBroadSword : ModItem
    {
        // 允许右键
        public override bool AltFunctionUse(Player player) => true;
        // 0=铜, 1=铁, 2=银
        private int paletteIndex = 1; // 默认铁
        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6f;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing; // 使用“持有型投射物”动画，由弹幕负责近战判定
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;

            // Item.noUseGraphic = true; // 不显示物品本体，由 held projectile 绘制/判定
            Item.noMelee = true;      // 物品本体不产生近战判定，改由弹幕造成伤害

            Item.shoot = ModContent.ProjectileType<SteelBroadSwordProjectile>();
            Item.shootsEveryUse = true;   // 确保每挥一次都会生成一次弹幕
            Item.shootSpeed = 0f;     // held projectile 通常不需要速度

            Item.value = Item.buyPrice(silver: 4);
            Item.rare = ItemRarityID.Green;
        }
        // 关键：像示例那样把方向、时长、缩放传进 ai[]
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo src, Vector2 pos, Vector2 vel, int type, int dmg, float kb)
        {
            float scale = player.GetAdjustedItemScale(Item);
            int proj = Projectile.NewProjectile(
                src,
                player.MountedCenter,
                new Vector2(player.direction, 0f), // 只提供“面向方向”
                type, dmg, kb, player.whoAmI,
                player.direction * player.gravDir, // ai[0]：左右（含重力翻转）
                player.itemAnimationMax,           // ai[1]：总时长
                scale                               // ai[2]：缩放
            );
            // 把配色塞进 localAI[1]
            if (proj >= 0 && proj < Main.maxProjectiles)
                Main.projectile[proj].localAI[1] = paletteIndex;
            return false;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                paletteIndex = (paletteIndex + 1) % 3;
                // 简单提示（可选）
                CombatText.NewText(player.getRect(),
                    paletteIndex == 0 ? new Color(205, 127, 50) : paletteIndex == 1 ? new Color(180, 180, 195) : new Color(192, 192, 192),
                    paletteIndex == 0 ? "铜" : paletteIndex == 1 ? "铁" : "银");
                return false; // 右键只切换，不挥砍
            }
            return base.CanUseItem(player);
        }
        public override void AddRecipes()
        {
            // 接受任意木头、铜锭、铁锭
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup(RecipeGroupID.Wood, 2);
            recipe.AddIngredient(ItemID.CopperBar, 2);
            recipe.AddIngredient(ItemID.IronBar, 2);
            recipe.AddIngredient(ItemID.EnchantedSword, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
