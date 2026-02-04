using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using System.Collections.Generic;

namespace WuDao.Content.Items.Weapons.Melee
{
    // 长生剑
    public class ImmortalSword : BuffItem
    {
        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6f;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;

            Item.value = Item.buyPrice(silver: 4);
            Item.rare = ItemRarityID.Green;
        }
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            rules.Add(new StatRule(BuffConditions.Always,
                StatEffect.MaxLife(20),
                StatEffect.LifeRegen(2)
            ));
        }
        public override void AddRecipes()
        {
            // 接受任意木头、铁锭
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup(RecipeGroupID.Wood, 2);
            recipe.AddIngredient(ItemID.Daybloom, 2);
            recipe.AddIngredient(ItemID.IronBar, 2);
            recipe.AddIngredient(ItemID.EnchantedSword, 1);
            recipe.AddIngredient(ItemID.LifeCrystal, 2);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
