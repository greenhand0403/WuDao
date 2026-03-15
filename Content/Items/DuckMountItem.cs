using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Mounts;

namespace WuDao.Content.Items
{
    public class DuckMountItem : ModItem
    {
        // vanilla miscEquips 里 mount 槽通常是 3
        private const int MountSlot = 3;
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


        public override bool CanRightClick()
        {
            return Item.stack == 1;
        }

        public override bool ConsumeItem(Player player)
        {
            return false;
        }

        public override void RightClick(Player player)
        {
            int invSlot = -1;

            for (int i = 0; i < 58; i++)
            {
                if (ReferenceEquals(player.inventory[i], Item))
                {
                    invSlot = i;
                    break;
                }
            }

            if (invSlot == -1)
                return;

            // 已经装着同一个物品就不处理
            if (!player.miscEquips[MountSlot].IsAir && player.miscEquips[MountSlot].type == Item.type)
                return;

            Item invItem = player.inventory[invSlot];
            Item mountItem = player.miscEquips[MountSlot];

            player.inventory[invSlot] = mountItem;
            player.miscEquips[MountSlot] = invItem;

            Main.mouseRightRelease = false;
            Recipe.FindRecipes();
        }
    }
}