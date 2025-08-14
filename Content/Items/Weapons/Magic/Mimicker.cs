using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Players;

namespace WuDao.Content.Items.Weapons.Magic
{
    // =====================
    // 2) 模仿者：Mimicker
    // - 初始仅能发射宝石法杖类射弹
    // - 用其射弹击败特定远程系敌怪，累计达到阈值后，解锁该敌怪对应“可用射弹”加入随机池
    // - 伤害随解锁种类数提升
    // =====================
    public class Mimicker : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 26;
            Item.rare = ItemRarityID.LightRed; // 困难模式早中期
            Item.value = Item.buyPrice(gold: 6);

            Item.DamageType = DamageClass.Magic;
            Item.damage = 22; // 初始略低，靠收集叠加
            Item.knockBack = 2f;
            Item.mana = 5;

            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item85;
            Item.noMelee = true;

            Item.shoot = ProjectileID.AmethystBolt; // 占位
            Item.shootSpeed = 10.5f;
            Item.autoReuse = true;
        }

        // 伤害随“已收集的射弹种类数”线性提升
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            var mp = player.GetModPlayer<MimickerPlayer>();
            int baseCount;
            int total = mp.TotalTypesUnlocked(out baseCount);
            int extraTypes = Math.Max(0, total - baseCount);
            // 每多一种+2% 伤害（可调）
            damage += 0.02f * extraTypes;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var mp = player.GetModPlayer<MimickerPlayer>();
            // 构建当前可用随机池（基础+解锁）
            var pool = new List<int>(mp.BuildCurrentPool());
            int choice = pool[Main.rand.Next(pool.Count)];

            Vector2 perturbed = velocity.RotatedByRandom(MathHelper.ToRadians(5));
            float speedScale = 0.95f + Main.rand.NextFloat(0.15f);
            int p = Projectile.NewProjectile(source, position, perturbed * speedScale, choice, damage, knockback, player.whoAmI);
            if (p >= 0 && p < Main.maxProjectiles)
            {
                Main.projectile[p].GetGlobalProjectile<MimickerGlobalProjectile>().fromMimicker = true;
            }
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer is Player p)
            {
                var mp = p.GetModPlayer<MimickerPlayer>();
                int baseCount;
                int total = mp.TotalTypesUnlocked(out baseCount);
                int extra = Math.Max(0, total - baseCount);
                tooltips.Add(new TooltipLine(Mod, "MimickerInfo", $"已收集射弹种类：{total}（基础 {baseCount}，额外 {extra}）"));
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Bone, 25)
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddIngredient(ItemID.CrystalShard, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}