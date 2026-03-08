using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using WuDao.Content.Projectiles.Summon;
using WuDao.Content.Buffs;

namespace WuDao.Content.Items.Weapons.Summon
{
    public class DandelionStaff : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Sunflower;
        public override void SetDefaults()
        {
            Item.damage = 16;

            Item.DamageType = DamageClass.Summon;

            Item.mana = 10;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 36;
            Item.useAnimation = 36;

            Item.useStyle = ItemUseStyleID.Swing;

            Item.knockBack = 2;

            Item.rare = ItemRarityID.Green;

            Item.UseSound = SoundID.Item44;

            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<DandelionSentry>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            position = Main.MouseWorld;

            Projectile.NewProjectile(source, position, Vector2.Zero,
                type, damage, knockback, player.whoAmI);

            player.UpdateMaxTurrets();

            return false;
        }
    }
}
