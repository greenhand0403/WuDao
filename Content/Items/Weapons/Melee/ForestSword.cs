using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using System.Collections.Generic;

namespace WuDao.Content.Items.Weapons.Melee
{
    // 茂林剑
    public class ForestSword : BuffItem
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
        protected override void BuildBuffRules(Player player, Item item, IList<BuffRule> rules)
        {
            rules.Add(new BuffRule(BuffConditions.Always,
                BuffEffect.PermanentBuff(BuffID.Swiftness), // ← 永久/不减时/无时间显示
                BuffEffect.PermanentBuff(BuffID.Sunflower)
            ));
        }
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            rules.Add(new StatRule(BuffConditions.InForest,
                StatEffect.RunAcceleration(0.1f) // 奔跑加速度 +10%
            ));
        }
        public override void AddRecipes()
        {
            // 接受任意木头、铜锭、铁锭
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup(RecipeGroupID.Wood, 2);
            recipe.AddIngredient(ItemID.SwiftnessPotion, 2);
            recipe.AddIngredient(ItemID.IronBar, 2);
            recipe.AddIngredient(ItemID.CopperShortsword, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
