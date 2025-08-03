using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

// This file is part of the Wu Dao mod for Terraria.
// It defines the Steel Heavy Sword item, which is a weapon in the game.
namespace WuDao.Items.Weapons
{
    public class SteelHeavySword : ModItem
    {
        public bool isRightClick = false;
        public override void SetDefaults()
        {
            Item.width = 40; // Width of the item
            Item.height = 40;

            Item.useStyle = ItemUseStyleID.Swing; // Style of use
            Item.useAnimation = 30; // Animation duration
            Item.useTime = 30; // Time taken to use the item
            Item.autoReuse = true; // Allows the item to be used repeatedly without clicking again

            Item.damage = 18; // Damage dealt by the sword
            Item.DamageType = DamageClass.Melee; // Type of damage
            Item.knockBack = 7f; // Knockback effect
            Item.crit = 8; // Critical hit chance

            Item.rare = ItemRarityID.Green; // Rarity of the item
            Item.value = Item.buyPrice(silver: 4); // Value of the item in gold
            Item.UseSound = SoundID.Item1; // Sound played when the item is used

            Item.useTurn = true; // Allows the player to turn while using the item
            Item.noUseGraphic = false; // Shows the item's graphic when used
            Item.noMelee = false;
        }
        public override bool AltFunctionUse(Player player) => true; // Allows the item to be used with an alternate function (like a special attack)
        public override void ModifyItemScale(Player player, ref float scale)
        {
            if (isRightClick)
                scale *= 1.5f; // 放大 1.5 倍
                               // else 不处理，保留默认 scale
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) // Check if the alternate function is being used
            {
                if (player.GetModPlayer<SteelHeavySwordPlayer>().RightClickCooldown <= 0) // Check if the cooldown is over
                {
                    isRightClick = true;
                    Item.useTime = 20;
                    Item.useAnimation = 20; // Set longer animation for the alternate use
                    Item.damage = 30;
                    Item.knockBack = 10f; // Increase knockback for the alternate use
                    Item.UseSound = SoundID.Item71; // Use the same sound for the alternate use
                }
                else
                {
                    return false; // Prevent using the item if the cooldown is not over
                }
            }
            else
            {
                isRightClick = false;
                Item.useTime = 30; // Reset to normal use time
                Item.useAnimation = 30; // Reset to normal animation time
                Item.damage = 18; // Reset to normal damage
                Item.knockBack = 7f; // Reset to normal knockback
                Item.UseSound = SoundID.Item1; // Use the same sound for the normal use
            }
            return base.CanUseItem(player); // Allow the item to be used
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3)) // 1 in 3 chance to trigger the effect
            {
                // Create a dust effect when the sword hits something
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.SilverFlame);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 判断是否是右键攻击
            if (player.altFunctionUse == 2)
            {
                if (Main.rand.NextFloat() < 0.3f)
                {
                    target.AddBuff(BuffID.Bleeding, 60 * 2); // 2秒流血
                }

                // 设置冷却
                player.GetModPlayer<SteelHeavySwordPlayer>().RightClickCooldown = 120; // 2秒冷却 (60 tick/s)
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 2)
                .AddIngredient(ItemID.CopperBar, 2)
                .AddIngredient(ItemID.IronBar, 2) // Requires 2 Copper Bars and 2 Iron Bars
                .AddTile(TileID.Anvils) // Crafted at an Anvil
                .Register(); // Register the recipe
        }
    }
}