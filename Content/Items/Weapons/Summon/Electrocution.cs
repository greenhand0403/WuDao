using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Items.Weapons.Summon
{
    public class Electrocution : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<ElectrocutionProj>(), 120, 3, 8);
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 4);
            Item.UseSound = SoundID.Item93;
        }

        public override bool MeleePrefix() => true;
    }
}