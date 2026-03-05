using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using WuDao.Common;
using System.Collections.Generic;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Items.Weapons.Melee
{
    // TODO: 蜂刺贴图
    public class BeeStings : BuffItem
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Melee/SteelShortSword";
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.useStyle = ItemUseStyleID.Rapier;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.autoReuse = false;

            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.damage = 30;
            Item.knockBack = 5;
            Item.crit = 4;

            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;

            Item.noUseGraphic = true; // The sword is actually a "projectile", so the item should not be visible when used
            Item.noMelee = true; // The projectile will do the damage and not the item
            Item.shoot = ModContent.ProjectileType<BeeStingsProjectile>(); // The projectile is what makes a shortsword work
            Item.shootSpeed = 2.1f; // This value bleeds into the behavior of the projectile as velocity, keep that in mind when tweaking values
        }
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            // 在丛林环境下，近战伤害 +5%
            rules.Add(new StatRule(BuffConditions.InJungle,
                StatEffect.MeleeDamageAdd(0.05f)
            ));
        }
        // 由丛林孢子、蜂蜡、毒刺、金短剑在铁砧处合成
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.JungleSpores, 4)
                .AddIngredient(ItemID.BeeWax, 4)
                .AddIngredient(ItemID.Stinger, 4)
                .AddIngredient(ItemID.GoldShortsword, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}