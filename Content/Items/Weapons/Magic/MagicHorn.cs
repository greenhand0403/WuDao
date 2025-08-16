using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
// TODO: 改贴图
namespace WuDao.Content.Items.Weapons.Magic
{
    // =====================
    // 1) 魔法号角：MagicHorn
    // 功能：发射随机射弹。困难模式前/后使用不同的随机池。
    // =====================
    public class MagicHorn : ModItem
    {
        // 预困难&困难模式的射弹池（可按需增删）
        private static readonly int[] PreHMProjectilePool = new int[]
        {
            ProjectileID.AmethystBolt,
            ProjectileID.TopazBolt,
            ProjectileID.SapphireBolt,
            ProjectileID.EmeraldBolt,
            ProjectileID.RubyBolt,
            ProjectileID.DiamondBolt,
            ProjectileID.WaterBolt, // 注意：此弹体会反弹，多段同屏限制可自行控制
            ProjectileID.MagicMissile,
        };

        private static readonly int[] HMProjectilePool = new int[]
        {
            ProjectileID.CrystalStorm,
            ProjectileID.CursedFlameFriendly,
            ProjectileID.GoldenShowerFriendly,
            ProjectileID.ShadowBeamFriendly,
            ProjectileID.SpectreWrath,
        };

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 26;
            Item.rare = ItemRarityID.Orange; // 同时代中后期魔法武器稀有度
            Item.value = Item.buyPrice(gold: 5);

            Item.DamageType = DamageClass.Magic;
            Item.damage = 24; // 参考同时期魔法武器的一个中等值
            Item.knockBack = 2.5f;
            Item.mana = 6;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item85; // 号角/喇叭类音效可自定
            Item.noMelee = true;

            Item.shoot = ProjectileID.WoodenArrowFriendly; // 仅作为默认占位
            Item.shootSpeed = 10f;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var pool = Main.hardMode ? HMProjectilePool : PreHMProjectilePool;
            int choice = pool[Main.rand.Next(pool.Length)];

            // 适度随机散射/速度微扰，增强手感
            Vector2 perturbed = velocity.RotatedByRandom(MathHelper.ToRadians(6));
            float speedScale = 0.9f + Main.rand.NextFloat(0.2f);

            int p = Projectile.NewProjectile(source, position, perturbed * speedScale, choice, damage, knockback, player.whoAmI);

            // 若弹体有独特AI可进一步处理（例如：修改本体ai或者本mod的GlobalProjectile）
            return false; // 返回false避免tML再次发射默认的shoot
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Bone, 20)
                .AddIngredient(ItemID.FallenStar, 5)
                .AddIngredient(ItemID.Book)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}