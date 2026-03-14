using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Items;
using WuDao.Content.Mounts;

namespace WuDao.Content.Players
{
    // 玩家退出鸭子坐骑时，有 5% 的概率丢失绿色蠕虫物品
    public class DuckMountPlayer : ModPlayer
    {
        private bool wasRidingDuckLastTick;

        public override void PostUpdate()
        {
            bool ridingDuckNow = Player.mount.Active &&
                                 Player.mount.Type == ModContent.MountType<DuckMount>();

            if (wasRidingDuckLastTick && !ridingDuckNow)
            {
                TryBreakGreenWormItem();
            }

            wasRidingDuckLastTick = ridingDuckNow;
        }

        private void TryBreakGreenWormItem()
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            if (Main.rand.NextFloat() >= 0.05f)
                return;

            int itemType = ModContent.ItemType<DuckMountItem>();

            for (int i = 0; i < Player.inventory.Length; i++)
            {
                Item item = Player.inventory[i];

                if (!item.IsAir && item.type == itemType)
                {
                    item.stack--;
                    if (item.stack <= 0)
                        item.TurnToAir();

                    if (Main.netMode != NetmodeID.Server)
                    {
                        Main.NewText(Language.GetTextValue("Mods.WuDao.Item.DuckMountItem.Broken"), 80, 220, 100);
                    }

                    break;
                }
            }
        }
    }
}