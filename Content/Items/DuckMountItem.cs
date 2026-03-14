using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Mounts;

namespace WuDao.Content.Items
{
    public class DuckMountItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 9999;

            Item.noMelee = true;
            Item.consumable = false;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item79;

            Item.noMelee = true;
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Green;

            Item.mountType = ModContent.MountType<DuckMount>();
            
            Item.bait = 30;
        }
    }
}