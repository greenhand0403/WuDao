using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Items.Weapons.Summon;

public class BirdWatchingLicense : ModItem
{
    public override string Texture => "Terraria/Images/Item_" + ItemID.SeagullCage;
    public override void SetDefaults()
    {
        Item.damage = 20;
        Item.mana = 10;

        Item.DamageType = DamageClass.Summon;

        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;

        Item.shoot = ModContent.ProjectileType<SeagullMinion>();
        Item.buffType = ModContent.BuffType<SeagullBuff>();

        Item.noMelee = true;
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        player.AddBuff(Item.buffType, 2);
        if (player.whoAmI == Main.myPlayer)
        {
            Projectile.NewProjectile(
            source,
            Main.MouseWorld,
            Vector2.Zero,
            type,
            damage,
            knockback,
            player.whoAmI);
        }

        return false;
    }
}