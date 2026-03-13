using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    public class CrackedCore : ModItem
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("破损核心");
        //     Tooltip.SetDefault(
        //         "增加10%魔法伤害\n" +
        //         "受到伤害后失效5秒\n" +
        //         "5秒未受伤则重新激活"
        //     );
        // }
        public override string Texture => "WuDao/Content/Items/Accessories/ApeTouch";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;

            Item.accessory = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(gold: 1);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<CrackedCorePlayer>().coreEquipped = true;

            // 如果未受伤标志位，则增加伤害
            if (player.GetModPlayer<CrackedCorePlayer>().coreCooldown <= 0)
            {
                player.GetDamage(DamageClass.Magic) += 0.10f;
            }
        }
    }
}