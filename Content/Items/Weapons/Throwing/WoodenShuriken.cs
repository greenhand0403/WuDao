using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Items.Weapons.Throwing
{
    // 木飞镖
    public class WoodenShuriken : BaseShurikenItem
    {
        // 数值配置
        protected override int BaseDamage => 6;
        protected override int BaseUseTime => 14;
        protected override int BaseUseAnimation => 14;
        protected override int BaseCrit => 2;

        protected override float BaseShootSpeed => 8f;
        protected override int Rarity => ItemRarityID.Green;
        protected override int ValueInCopper => Item.buyPrice(copper: 1);

        // tModLoader 1.4 建议改成 DamageClass.Ranged
        protected override DamageClass DmgClass => DamageClass.Throwing;

        // 绑定它的投射物
        protected override int ProjectileType => ModContent.ProjectileType<WoodenShurikenProjectile>();

        // 配方（30 个/组）
        protected override void BuildRecipe(Recipe recipe)
        {
            recipe.AddIngredient(ItemID.Wood, 2);
        }
    }
}
