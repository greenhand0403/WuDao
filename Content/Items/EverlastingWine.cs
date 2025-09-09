using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items
{
    public class EverlastingWine : ModItem
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("永生之酒");
        //     Tooltip.SetDefault("饮用：永久减少20点生命上限（可用生命水晶弥补）\n" +
        //                         "50% 几率获得2秒无敌\n" +
        //                         "饮用冷却：5分钟\n" +
        //                         "背包中有此物且冷却就绪时，在遭受致命伤害前会自动饮用并躲过该次伤害");
        // }
        public override string Texture => $"Terraria/Images/Item_{ItemID.CreamSoda}";

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.consumable = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(silver: 5);
            Item.UseSound = SoundID.Item3;
        }

        public override bool CanUseItem(Player player)
        {
            // 冷却中不可用
            return !player.HasBuff(ModContent.BuffType<Content.Buffs.WineCooldownBuff>());
        }

        public override bool? UseItem(Player player)
        {
            var mp = player.GetModPlayer<PotionPlayer>();
            // 直接施加效果（见 ModPlayer 的逻辑）
            mp.GetType().GetMethod("ConsumeEverlastingWine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              ?.Invoke(mp, new object[] { true });

            return true;
        }

        public override void AddRecipes()
        {
            // 自定义配方：瓶装水 + 死亡草 + 萤火虫（象征“诡异/永生”气息），随意改
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Deathweed, 1)
                .AddIngredient(ItemID.Firefly, 5)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}
