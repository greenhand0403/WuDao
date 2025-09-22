using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Items.Weapons.Throwing
{
    /// <summary>
    /// 可继承的飞镖物品基类：
    /// - 统一克隆原版手感
    /// - 通过虚属性定制数值
    /// - 通过虚方法定制配方与发射行为
    /// </summary>
    public abstract class BaseShurikenItem : ModItem
    {
        // ====== 可在子类里覆写的“配置项” ======
        protected virtual int BaseDamage => 10;
        protected virtual int BaseUseTime => 16;
        protected virtual int BaseUseAnimation => 16;
        protected virtual int BaseCrit => 0;

        protected virtual float BaseShootSpeed => 9f;
        protected virtual int Rarity => ItemRarityID.Green;
        protected virtual int ValueInCopper => Item.buyPrice(silver: 15);

        /// <summary>子类必须返回对应的投射物类型。</summary>
        protected abstract int ProjectileType { get; }

        /// <summary>子类想使用 Ranged/Throwing 等，自行覆写。</summary>
        protected virtual DamageClass DmgClass => DamageClass.Throwing; // 1.4 建议改为 DamageClass.Ranged

        // ====== 核心默认值 ======
        public override void SetDefaults()
        {
            // 统一克隆原版镖类手感（可消耗、轨迹、使用样式等）
            Item.CloneDefaults(ItemID.Shuriken);

            Item.DamageType = DmgClass;
            Item.damage = BaseDamage;
            Item.useTime = BaseUseTime;
            Item.useAnimation = BaseUseAnimation;
            Item.autoReuse = true;

            Item.crit = BaseCrit;
            Item.rare = Rarity;
            Item.value = ValueInCopper;

            Item.shoot = ProjectileType;
            Item.shootSpeed = BaseShootSpeed;
        }

        // ====== 发射前微调（速度、类型、散布等）=====
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // 允许子类进一步定制（例如多发、扩散、随机速度）
            ModifyShootStatsVirtual(player, ref position, ref velocity, ref type, ref damage, ref knockback);
        }

        /// <summary>给子类覆写的扩展点。</summary>
        protected virtual void ModifyShootStatsVirtual(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

        // ====== 配方 ======
        public override void AddRecipes()
        {
            // 基类提供一个“批量 33 个”的默认模板；子类自由增删材料
            var recipe = CreateRecipe(33);
            BuildRecipe(recipe);
            recipe.Register();
        }

        /// <summary>子类实现：往 recipe 填入材料与合成台。</summary>
        protected abstract void BuildRecipe(Recipe recipe);
    }
}
