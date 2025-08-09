using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items.Weapons.Throwing.Base;

namespace WuDao.Content.Items.Weapons.Throwing
{
    public class PoisonedShuriken : BaseShurikenItem
    {
        // 数值配置
        protected override int BaseDamage => 14;
        protected override int BaseUseTime => 16;
        protected override int BaseUseAnimation => 16;
        protected override int BaseCrit => 2;

        protected override float BaseShootSpeed => 9f;
        protected override int Rarity => ItemRarityID.Green;
        protected override int ValueInCopper => Item.buyPrice(silver: 15);

        // tModLoader 1.4 建议改成 DamageClass.Ranged
        protected override DamageClass DmgClass => DamageClass.Throwing;

        // 绑定它的投射物
        protected override int ProjectileType => ModContent.ProjectileType<Projectiles.Throwing.PoisonedShurikenProjectile>();

        // 配方（30 个/组）
        protected override void BuildRecipe(Recipe recipe)
        {
            recipe.AddIngredient(ItemID.JungleSpores, 5);
            recipe.AddIngredient(ItemID.IronBar, 2);
            recipe.AddTile(TileID.Anvils);
        }
    }
}
