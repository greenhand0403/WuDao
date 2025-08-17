// Content/Items/Weapons/InvincibleBlade.cs
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Cooldowns;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Items.Weapons.Melee
{
    // TODO: 贴图 以及百分比伤害未测试
    public class InvincibleBlade : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Zenith}";
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
            Item.noMelee = true;
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
            var mp = player.GetModPlayer<InvincibleBladeCooldown>();
            return mp.Cooldown <= 0; // 冷却未到则可用
        }

        public override bool? UseItem(Player player)
        {
            // 设定自定义冷却（不吃攻速）
            player.GetModPlayer<InvincibleBladeCooldown>().Cooldown = InvincibleBlade.CooldownFrames;
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.whoAmI != Main.myPlayer) return true;

            // 在鼠标附近随机生成
            Vector2 spawnPos = Main.MouseWorld + 150 * (new Vector2(Main.rand.NextFloat(-1, 1f), Main.rand.NextFloat(-1, 1f)));

            // 发射的射弹随着使用时间增多
            int cold = player.GetModPlayer<InvincibleBladeCooldown>().Cooldown;
            for (int i = 0; i < 1 + (InvincibleBlade.CooldownFrames - cold) / 2; i++)
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

            return false;
        }
    }
}
