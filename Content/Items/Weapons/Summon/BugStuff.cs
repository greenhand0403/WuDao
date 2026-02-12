using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Summon;
using WuDao.Content.Buffs;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Items.Weapons.Summon
{
    public class BugStuff : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.SlimeStaff;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            // 占 1 个召唤栏（保持与刃杖一致）
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.SlimeStaff);

            Item.damage = 10; // 略强于雀杖/史莱姆杖
            Item.knockBack = 2.5f;
            Item.mana = 10;

            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<GrasshopperMinion>();
            Item.buffType = ModContent.BuffType<GrasshopperBuff>();

            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item44;
            Item.value = Item.buyPrice(0, 0, 50);
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            // position = Main.MouseWorld;
            return true;
        }
    }
}