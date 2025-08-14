using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Buffs;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Items.Weapons.Melee
{
    public class SteelShortSword : BuffItem
    {
        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.knockBack = 4f;
            Item.useStyle = ItemUseStyleID.Rapier; // Makes the player do the proper arm motion
            Item.useAnimation = 12;
            Item.useTime = 12;
            Item.width = 32;
            Item.height = 32;
            Item.UseSound = SoundID.Item1;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.autoReuse = false;
            Item.noUseGraphic = true; // The sword is actually a "projectile", so the item should not be visible when used
            Item.noMelee = true; // The projectile will do the damage and not the item

            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(0, 0, 3, 0);

            Item.shoot = ModContent.ProjectileType<SteelShortSwordProjectile>(); // The projectile is what makes a shortsword work
            Item.shootSpeed = 3f; // This value bleeds into the behavior of the projectile as velocity, keep that in mind when tweaking values
        }
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            // 白天的小幅移动强化
            rules.Add(new StatRule(BuffConditions.DayTime,
                StatEffect.RunSpeed(0.5f),        // maxRunSpeed +0.5
                StatEffect.RunAcceleration(0.08f) // runAcceleration +0.08
            ));
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 2)
                .AddIngredient(ItemID.CopperBar, 2)
                .AddIngredient(ItemID.IronBar, 2)
                .AddIngredient(ItemID.Sunflower, 2)
                .AddIngredient(ItemID.SwiftnessPotion, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }

    }
}