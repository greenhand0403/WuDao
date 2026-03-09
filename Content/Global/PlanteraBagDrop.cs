using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using WuDao.Content.Items.Weapons.Ranged;

namespace WuDao.Content.Global
{
    public class PlanteraBagDrop : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            if (item.type == ItemID.PlanteraBossBag)
            {
                itemLoot.Add(ItemDropRule.Common(
                    ModContent.ItemType<SeedMinigun>(),
                    8
                ));
            }
        }
    }
}