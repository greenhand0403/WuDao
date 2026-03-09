using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    public class RoundStonePillar : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;

            Item.accessory = true;

            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;

            Item.defense = 5;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RoundStonePillarPlayer>().roundStonePillar = true;

            // 下坠更快
            player.maxFallSpeed += 6f;
            player.gravity += 0.3f;

            // 降低移动
            player.moveSpeed -= 0.35f;
            player.accRunSpeed -= 1.5f;

            // 降低跳跃
            player.jumpSpeedBoost -= 3f;
        }
    }
}
