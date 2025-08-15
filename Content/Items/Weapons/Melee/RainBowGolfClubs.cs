using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Players;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Items.Weapons.Melee
{
    public class RainBowGolfClubs : ModItem
    {
        public override void SetDefaults()
        {
            // 近战武器参数（你可按需平衡）
            Item.damage = 42;
            Item.DamageType = DamageClass.MeleeNoSpeed; // 近战但不吃攻速作伤害加成
            Item.knockBack = 6f;

            Item.useStyle = ItemUseStyleID.Swing; // 挥动
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.autoReuse = true;

            Item.width = 40;
            Item.height = 40;

            Item.noMelee = true;       // 不用近战挥击盒子造成伤害，伤害由射弹承担
            Item.noUseGraphic = false; // 仍显示武器贴图

            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Pink;

            Item.UseSound = SoundID.Item1;

            Item.shoot = ModContent.ProjectileType<RainbowGolfBall>();
            Item.shootSpeed = 12f; // 基础朝向速度；实际会在 Shoot() 里做随机化
        }

        public override bool CanUseItem(Player player)
        {
            // 这里保持默认可用。若你想防止贴墙发射可加判定
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // —— 颜色索引循环（对应 4242..4255）——
            var cycle = player.GetModPlayer<RainbowGolfCyclePlayer>();
            int colorIndex = cycle.NextIndex();

            // —— 随机速度与偏角 —— 
            // 以玩家朝向 (velocity) 为基准，加入 ±12 度随机偏转，速度 9..15 随机
            float baseSpeed = velocity.Length();
            if (baseSpeed <= 0.01f) baseSpeed = Item.shootSpeed;

            float speed = Main.rand.NextFloat(9f, 15f);
            float maxAngleDeg = 12f;
            float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-maxAngleDeg, maxAngleDeg));

            Vector2 dir = velocity.SafeNormalize(player.direction == 1 ? Vector2.UnitX : -Vector2.UnitX);
            Vector2 finalVel = dir.RotatedBy(angleOffset) * speed;

            // —— 发射弹体 —— 
            int proj = Projectile.NewProjectile(
                source,
                position,
                finalVel,
                type,            // RainbowGolfBall
                damage,
                knockback,
                player.whoAmI,
                ai0: colorIndex   // 用 ai[0] 传颜色索引到弹体
            );

            // 让弹体继承近战暴击率（可选）
            Main.projectile[proj].CritChance = player.GetWeaponCrit(Item);

            // 可选：播放更清脆的击球声
            SoundEngine.PlaySound(SoundID.Item1 with { PitchVariance = 0.2f, Volume = 0.9f }, position);

            // 返回 false：阻止 tML 再基于 Item.shoot/Item.shootSpeed 自动发一个
            return false;
        }
    }
}
