using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Items.Weapons.Summon
{
    public class FishSchoolWhip : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<FishSchoolWhipProj>(), 26, 2, 4);
            Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(0, 2));
        }
    }
}