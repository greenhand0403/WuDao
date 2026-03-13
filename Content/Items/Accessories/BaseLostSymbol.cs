using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    // 失落符文
    public abstract class BaseLostSymbol : ModItem
    {
        public override string Texture => "WuDao/Content/Items/Accessories/LostSymbol";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(gold: 1);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<LostSymbolPlayer>().LostSymbolCount++;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "LostSymbolInfo",
                "每装备1个失落符文，增加1%全伤害"));
        }

        public override void AddRecipes()
        {
            Recipe.Create(Type)
                .AddIngredient(ItemID.Shackle, 1)
                .AddIngredient(ItemID.IronBar, 8)
                .AddIngredient(ItemID.Ruby, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}