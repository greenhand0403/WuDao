using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;
using WuDao.Content.Players;

namespace WuDao.Content.Items
{
    public class EverlastingWine : ModItem
    {
        // TODO: 喝酒扣除生命上限后无法再通过吃生命水晶加去。触发复活后，生命再生好像变为0了，不会自然再生恢复生命
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
            return !player.HasBuff(ModContent.BuffType<WineCooldownBuff>());
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
