// Content/Items/Weapons/InvincibleBlade.cs
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Items.Weapons.Melee
{
    public class InvincibleBlade : ModItem
    {
        // 固定冷却（帧），不受攻速影响
        public const int CooldownFrames = 10; // 约 1/6 秒，可自行调

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 10;         // 形参而已，实际由冷却控制
            Item.useAnimation = 10;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item60;
            // Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<InvincibleArcShot>();
            Item.shootSpeed = 1f;      // 由射弹自身逻辑决定
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(0, 50, 0, 0);
            Item.DamageType = DamageClass.Melee; // 外观剑型
            Item.damage = 1;           // 无效占位，实际伤害在射弹里计算
            Item.knockBack = 0f;
            Item.channel = false;
        }

        public override bool CanUseItem(Player player)
        {
            var mp = player.GetModPlayer<InvincibleBladePlayer>();
            return mp.Cooldown <= 0; // 冷却未到则可用
        }

        public override bool? UseItem(Player player)
        {
            // 设定自定义冷却（不吃攻速）
            player.GetModPlayer<InvincibleBladePlayer>().Cooldown = CooldownFrames;
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.whoAmI != Main.myPlayer) return true;

            // 在鼠标附近随机生成
            Vector2 spawnPos = Main.MouseWorld + 150 * (new Vector2(Main.rand.NextFloat(-1, 1f), Main.rand.NextFloat(-1, 1f)));
            InvincibleBladePlayer mp = player.GetModPlayer<InvincibleBladePlayer>();
            // 发射的射弹随着使用时间增多
            int cold = mp.Cooldown;
            for (int i = 0; i < 1 + (CooldownFrames - cold) / 2; i++)
            {
                // 发射一个“弧线寻敌”的射弹，速度/路径由射弹自行处理
                Projectile.NewProjectile(
                    new EntitySource_ItemUse(player, Item),
                    spawnPos,
                    Vector2.Zero, // 初速由射弹自己决定
                    ModContent.ProjectileType<InvincibleArcShot>(),
                    Item.damage, 0f, player.whoAmI, 0, 0, 0
                );
            }
            if (mp.ExtraSpawnCD <= 0)
            {
                mp.ExtraSpawnCD = 60; // 60帧=1秒
                Vector2 spawnPos2 = Main.MouseWorld + 150 * new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                // 这里用 ai[1] 传一个“负数的 ItemID”告诉弹体：不要随机，直接用这把剑的贴图
                float forceItemId = -ItemID.Zenith; // 你的本体现在用的是 Zenith 贴图
                Projectile.NewProjectile(
                    source,
                    spawnPos2,
                    Vector2.Zero,
                    ModContent.ProjectileType<InvincibleArcShot>(),
                    Math.Max(1, Item.damage),
                    0f,
                    player.whoAmI,
                    0f,
                    forceItemId, // ← ai[1] 传负的 ItemID，下面改弹体去识别
                    0f
                );
            }
            return false;
        }
    }
}
