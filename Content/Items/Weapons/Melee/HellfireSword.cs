using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Buffs;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Items.Weapons.Melee
{
    public class HellfireSword : BuffItem
    {
        // TODO: 重绘贴图 地狱刀合成表
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

            Item.shoot = ModContent.ProjectileType<HellfireSwordProjectile>(); // ID of the projectiles the sword will shoot
            Item.shootSpeed = 8f; // Speed of the projectiles the sword will shoot

            // If you want melee speed to only affect the swing speed of the weapon and not the shoot speed (not recommended)
            // Item.attackSpeedOnlyAffectsWeaponAnimation = true;

            // Normally shooting a projectile makes the player face the projectile, but if you don't want that (like the beam sword) use this line of code
            // Item.ChangePlayerDirectionOnShoot = false;
        }

        protected override void BuildBuffRules(Player player, Item item, IList<BuffRule> rules)
        {
            rules.Add(new BuffRule(BuffConditions.Always,
                // 狱火药水的 Inferno（环火）效果，持续顶时间
                new BuffEffect(BuffID.Inferno, topUpAmount: 180, refreshThreshold: 30)
            ));
        }
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            rules.Add(new StatRule(BuffConditions.Always,
                // 手持时免疫“着火了/燃烧/岩浆”（下面第3点用得到）
                StatEffect.ImmuneTo(BuffID.OnFire, BuffID.Burning),
                StatEffect.LavaImmune()
            ));
        }
        // 武器本体挥砍命中
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 25% 几率上“着火了”（2 秒 = 120 帧）
            if (Main.rand.NextFloat() < 0.25f)
                target.AddBuff(BuffID.OnFire, 120);
        }
        // 武器本体挥砍命中时，在地狱区域额外增加 20% 伤害
        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (player.ZoneUnderworldHeight)
                modifiers.FinalDamage *= 1.20f; // +20% 最终伤害
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HellstoneBar, 20)
                .AddTile(TileID.Hellforge);
        }
    }
}
