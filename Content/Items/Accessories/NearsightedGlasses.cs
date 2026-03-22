using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    /// <summary>
    /// 近视眼镜 饰品
    /// TODO: 未测试
    /// 单机下切换饰品隐藏显示，看圆环是否消失，但实际增伤/减伤仍然存在。
    /// 联机下客户端装备近视眼镜，被敌怪发射物命中时，看减伤是否真实生效。
    /// 联机下不同玩家分别装备/不装备，看是否只影响装备者自己
    /// </summary>
    public class NearsightedGlasses : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 3, 0, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var gp = player.GetModPlayer<NearsightedPlayer>();
            gp.Nearsighted = true;
            gp.ShowRangeRings = !hideVisual;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Goggles)
                .AddIngredient(ItemID.HellstoneBar, 2)
                .AddTile(TileID.Hellforge)
                .Register();
        }
    }
}
