using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Items.Weapons.Melee
{
    // TODO: 爪套需要重做
    public class GlitteringClaw : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("闪光爪套");
            // Tooltip.SetDefault("贴脸迅击的爪套武器");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.DamageType = DamageClass.Melee;
            Item.damage = 95;              // 自行平衡
            Item.knockBack = 3.25f;
            Item.crit = 4;

            Item.useStyle = ItemUseStyleID.Shoot; // 用“持有/射击”风格生成 Holdout
            Item.useTime = 7;                      // 很快的出手（连打）
            Item.useAnimation = 7;
            Item.autoReuse = true;
            Item.channel = false;                  // 单拍；如需长按保持可改 true 并延长弹幕寿命

            Item.noUseGraphic = true;              // 不显示物品贴图；显示由弹幕负责
            Item.noMelee = true;                   // 本体不造成伤害，由弹幕打击

            Item.shoot = ModContent.ProjectileType<GlitteringClawHoldout>();
            Item.shootSpeed = 1f;                  // 由弹幕自身AI控制，不依赖此速度

            Item.value = Item.sellPrice(gold: 6);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 生成一个“短生命”的持有弹幕；把方向传过去
            Vector2 aim = (Main.MouseWorld - player.MountedCenter);
            if (aim.LengthSquared() < 0.001f)
                aim = new Vector2(player.direction, 0f);

            aim.Normalize();

            int proj = Projectile.NewProjectile(
                source,
                player.MountedCenter,
                aim, // 初速度无所谓，AI里会接管
                type,
                damage,
                knockback,
                player.whoAmI
            );

            // 可传递初始旋转与连击段位等自定义参数
            Main.projectile[proj].rotation = aim.ToRotation();

            return false; // 不由 tML 再生成默认弹幕
        }
    }
}
