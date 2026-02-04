using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items
{
    // 静功秘籍 永久提升静止不动时的气力恢复速度
    public class QiSecretsA : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Book}";
        public const float BonusPerUse = 2f;  // 每次+2点/秒
        public const int MaxUses = 4;          // 最多使用2次
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(0, 1);
            Item.maxStack = 99;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.UseSound = SoundID.Item4;
        }

        public override bool CanUseItem(Player player)
        {
            var qi = player.GetModPlayer<QiPlayer>();
            return qi.JinggongUsed < MaxUses;
        }

        public override bool? UseItem(Player player)
        {
            var qi = player.GetModPlayer<QiPlayer>();
            qi.JinggongUsed++;
            qi.QiRegenStandBonus += BonusPerUse;

            if (player.whoAmI == Main.myPlayer)
                Main.NewText($"静功有成！静止回气 +{BonusPerUse}/s（{qi.JinggongUsed}/{MaxUses}）");

            return true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ManaCrystal,10)
                .AddIngredient(ItemID.Book)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}