using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Items.Weapons.Throwing
{
    public class SmokeGrenade : ModItem
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("烟雾手雷");
        //     Tooltip.SetDefault("爆炸后留下 5 秒烟雾，对经过的敌怪每秒造成 5~10 点固定伤害");
        // }

        public override void SetDefaults()
        {
            Item.damage = 0; // 伤害由烟雾造成
            Item.DamageType = DamageClass.Throwing;
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 0f;
            Item.value = Item.buyPrice(0, 0, 50);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<SmokeGrenadeProj>();
            Item.shootSpeed = 8f;
            Item.consumable = true;
            Item.maxStack = 999;
        }
    }
}
