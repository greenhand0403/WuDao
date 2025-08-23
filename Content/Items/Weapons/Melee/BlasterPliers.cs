using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Wudao.Content.Items.Weapons.Melee
{
    // TODO: 重绘贴图 爆破钳
    class BlasterPliers : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.CombatWrench}";
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.CombatWrench);
            Item.noMelee = false;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shoot = ProjectileID.None;
            Item.noUseGraphic = false;
        }
        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            int g = target.rarity;
            if (g > 0)
            {
                float multiplier = 1f + g * 0.5f;
                modifiers.FinalDamage *= multiplier;

                CombatText.NewText(
                    target.Hitbox,
                    new Color(180, 220, 255),
                    $"敌怪稀有度 {g}"
                );
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.OldShoe)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}