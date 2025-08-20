using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Items.Weapons.Throwing
{
    // 继承飞镖基类的火飞镖
    public class FlameShuriken : BaseShurikenItem
    {
        // 数值配置
        protected override int BaseDamage => 12;
        protected override int BaseUseTime => 18;
        protected override int BaseUseAnimation => 18;
        protected override int BaseCrit => 2;

        protected override float BaseShootSpeed => 8f;
        protected override int Rarity => ItemRarityID.Green;
        protected override int ValueInCopper => Item.buyPrice(silver: 10);

        // tModLoader 1.4 建议改成 DamageClass.Ranged
        protected override DamageClass DmgClass => DamageClass.Throwing;

        // 绑定它的投射物
        protected override int ProjectileType => ModContent.ProjectileType<FlameShurikenProjectile>();

        // 配方（30 个/组）
        protected override void BuildRecipe(Recipe recipe)
        {
            recipe.AddIngredient(ItemID.Torch, 10);
            recipe.AddIngredient(ItemID.IronBar, 2);
            recipe.AddTile(TileID.Anvils);
        }
    }
}
