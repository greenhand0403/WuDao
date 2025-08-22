using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Buffs;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Items.Weapons.Melee
{
    // TODO: 改贴图 玄铁短剑 射弹的碰撞箱在中间不在剑尖是 bug
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
            Item.DamageType = DamageClass.Melee;
            Item.autoReuse = true;
            Item.noUseGraphic = true; // The sword is actually a "projectile", so the item should not be visible when used
            Item.noMelee = true; // The projectile will do the damage and not the item

            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 0, 3, 0);

            Item.shoot = ModContent.ProjectileType<SteelShortSwordProjectile>(); // The projectile is what makes a shortsword work
            Item.shootSpeed = 3f; // This value bleeds into the behavior of the projectile as velocity, keep that in mind when tweaking values
        }
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            // TODO: 测试其他手持武器加成效果 白天的增加15%移动速度 增加15%攻击速度 增加20%暴击率 增加10%伤害
            rules.Add(new StatRule(BuffConditions.Always,
            // 闪电靴
            // StatEffect.MoveSpeed(0.08f),
            // StatEffect.accRunSpeed(6.75f)
            // 直接把最大速度的系数增加，默认是3f，增加3.75f后变成6.75f相当于闪电靴的速度
            // StatEffect.AccRunSpeed(3.75f)
            // 暗影盔甲套装
            // StatEffect.RunAcceleration(0.75f),
            // StatEffect.AccRunSpeed(0.15f),
            // StatEffect.MaxRunSpeed(0.15f),
            // StatEffect.RunSlowdown(0.75f)
            // 蛙腿
            // StatEffect.ControlJump(true),
            // StatEffect.JumpSpeedBoost(1.6f),
            // StatEffect.ExtraFall(10)
            // 气球
            // StatEffect.JumpBoost()
            // 机械手套
            // StatEffect.KnockbackMulti(1f),//+100%击退
            // StatEffect.kbGlove(),//启用原版标志位 +100%击退
            // StatEffect.MeleeDamageAdd(0.12f),//+12%近战伤害
            // StatEffect.MeleeAttackSpeedAdd(0.12f),//+12%近战攻速
            // StatEffect.MeleeSizePercent(0.1f),//+10%近战武器尺寸
            // StatEffect.MeleeScaleGlove(),//启用原版标志位 +10%近战武器尺寸
            // StatEffect.AutoReuse()//启用原版标志位 自动重复攻击
            // StatEffect.ImmuneTo(BuffID.OnFire),//熔岩盔甲套装奖励 免疫火 buff
            // StatEffect.EnduranceAdd(0.1f)//+10%耐力
            // StatEffect.FireWalk(),//熔岩护身符
            // StatEffect.LavaMaxAdd(420)
            StatEffect.LavaImmune(),
            StatEffect.NoKnockback()
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