using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using WuDao.Content.Projectiles.Summon;
using WuDao.Content.Buffs;

namespace WuDao.Content.Items.Weapons.Summon
{
    // 像太极剑一样做个改动，只能召唤两只，一只僵尸新郎，一只僵尸新娘
    public class ZombieStaff : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 12;
            Item.DamageType = DamageClass.Summon;

            Item.mana = 10;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 36;
            Item.useAnimation = 36;

            Item.useStyle = ItemUseStyleID.Swing;

            Item.knockBack = 2;

            Item.value = Item.buyPrice(0, 0, 50);
            Item.rare = ItemRarityID.Blue;

            Item.UseSound = SoundID.Item44;

            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<ZombieMinion>();
            Item.buffType = ModContent.BuffType<ZombieMinionBuff>();

            Item.shootSpeed = 10f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            Projectile.NewProjectile(source, position, velocity,
                type, damage, knockback, player.whoAmI);

            return false;
        }
    }
}