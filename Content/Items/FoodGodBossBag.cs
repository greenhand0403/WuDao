using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using WuDao.Content.Items.Weapons.Melee;
using WuDao.Content.Items.Accessories;

namespace WuDao.Content.Items
{
    public class FoodGodBossBag : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.KingSlimeBossBag}";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Type] = true;
            ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;
            Item.ResearchUnlockCount = 3;
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.consumable = true;
            Item.maxStack = Item.CommonMaxStack;
        }
        public override bool CanRightClick()
        {
            return true;
        }
        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<ScallionSword>(), 7));
            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<ScallionShield>(), 7));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SteamedBun>(), 1, 2, 16));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Candy>(), 1, 1, 5));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<GlowingMeal>(), 1, 1, 2));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<FoodGodSummonItem>(), 1, 1));
        }
    }
}