using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Buffs;
using WuDao.Content.Projectiles.Melee;
using System.Collections.Generic;

namespace WuDao.Content.Items.Weapons.Melee
{
    public class OceanBlueSword : BuffItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.autoReuse = true;

            Item.DamageType = DamageClass.Melee;
            Item.damage = 35;
            Item.knockBack = 6;
            Item.crit = 6;

            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;

            Item.shoot = ModContent.ProjectileType<OceanBlueSwordProjectile>(); // ID of the projectiles the sword will shoot
            Item.shootSpeed = 8f; // Speed of the projectiles the sword will shoot

            // If you want melee speed to only affect the swing speed of the weapon and not the shoot speed (not recommended)
            // Item.attackSpeedOnlyAffectsWeaponAnimation = true;

            // Normally shooting a projectile makes the player face the projectile, but if you don't want that (like the beam sword) use this line of code
            // Item.ChangePlayerDirectionOnShoot = false;
        }
        // ① 手持时给：水下呼吸（Gills）+ 游泳（Flipper）+ 水上漂（WaterWalking）
        protected override void BuildBuffRules(Player player, Item item, IList<BuffRule> rules)
        {
            rules.Add(new BuffRule(BuffConditions.Always,
                new BuffEffect(BuffID.Gills, topUpAmount: 180, refreshThreshold: 30),
                new BuffEffect(BuffID.Flipper, topUpAmount: 180, refreshThreshold: 30),
                new BuffEffect(BuffID.WaterWalking, topUpAmount: 180, refreshThreshold: 30)
            ));
        }
        // ② 手持时免疫：霜冻（Frostburn）与 冰冻（Frozen）
        //    （如需也免疫“寒冷(Chilled)”，把 BuffID.Chilled 也加进去）
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            rules.Add(new StatRule(BuffConditions.Always,
                StatEffect.ImmuneTo(BuffID.Frostburn, BuffID.Frozen),
                StatEffect.ImmuneTo(BuffID.Chilled) // 可选：免疫“寒冷”
            ));
        }
        // ③ 武器本体命中：25% 几率施加“霜冻”（2 秒 = 120 帧）
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.25f)
                target.AddBuff(BuffID.Frostburn, 120);
        }
        // ④ 海洋环境增伤：在沙滩/海边（ZoneBeach）时 +20% 最终伤害
        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (player.ZoneBeach)
                modifiers.FinalDamage *= 1.20f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HellfireSword>(), 1) // 使用地狱之锋作为材料
                .AddCondition(Condition.NearWater)   // 必须在水边才能合成
                .Register();
        }
    }
}
