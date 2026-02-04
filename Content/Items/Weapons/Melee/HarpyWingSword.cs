using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using System.Collections.Generic;

namespace WuDao.Content.Items.Weapons.Melee
{
    public class HarpyWingSword : BuffItem
    {
        // 翼剑
        public override float UseTimeMultiplier(Player player) => IsAirborne(player) ? 1.10f : 1f;           // 空中 +10% 使用速度

        public override float UseAnimationMultiplier(Player player) => IsAirborne(player) ? 1.10f : 1f;
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Melee;
            Item.damage = 14;
            Item.knockBack = 6f;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing; // 使用“持有型投射物”动画，由弹幕负责近战判定
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;

            Item.noMelee = false;

            Item.shoot = ProjectileID.HarpyFeather;

            Item.shootSpeed = 10f;

            Item.value = Item.buyPrice(silver: 4);
            Item.rare = ModContent.RarityType<LightBlueRarity>();
        }
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            rules.Add(new StatRule(BuffConditions.Always,
                StatEffect.SlowFall(),
                StatEffect.NoFallDmg()
            ));
        }
        static bool IsAirborne(Player p)
        {
            // 1) 竖直速度不为 0，大概率在空中（跳/坠/飞）
            if (p.velocity.Y != 0f)
                return true;

            // 2) 扫描“脚下”一行的方块（Hitbox 底缘 + 1 像素）
            Rectangle hb = p.Hitbox; // 等价 new Rectangle((int)p.position.X, (int)p.position.Y, p.width, p.height)
            int feetY = (hb.Bottom + 1) / 16;
            int left = hb.Left / 16;
            int right = (hb.Right - 1) / 16;

            for (int x = left; x <= right; x++)
            {
                var tile = Framing.GetTileSafely(x, feetY);
                if (!tile.HasUnactuatedTile) continue;

                bool isSolid = Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]; // 实心砖
                bool isPlatform = Main.tileSolidTop[tile.TileType];                                   // 平台/半砖顶

                if (isSolid)
                    return false; // 正站在实心砖上 → 非空中

                if (isPlatform)
                {
                    // 只有在“不是按着下蹲去穿平台”的情况下，才把平台视为地面
                    if (p.gravDir == 1f)
                    {
                        if (!p.controlDown) // 正常重力且没有按↓
                            return false;
                    }
                    else // 倒重力
                    {
                        if (!p.controlUp)   // 倒重力时按↑穿平台
                            return false;
                    }
                }
            }

            // 脚下无砖/在按↓准备穿平台 → 空中
            return true;
        }
        public override void ModifyItemScale(Player player, ref float scale)
        {
            if (IsAirborne(player))
                scale *= 1.3f; // 空中挥砍体积 +30%
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float speedMult = 1f;
            float scaleMult = 1f;
            float dmgMult = 1f;

            if (IsAirborne(player))
            {
                speedMult = 1.3f; // 飞行速度 +30%
                scaleMult = 1.3f; // 尺寸     +30%（需要自行在弹幕上读 scale）
                dmgMult = 1.3f; // 伤害     +30%
            }

            // 生成射弹（你也可以直接 return false 并手动 NewProjectile 完全自定义）
            // 射弹伤害改为武器伤害 * 伤害乘子 未做平衡性考量
            int proj = Projectile.NewProjectile(
                source,
                position,
                velocity * speedMult,
                type,
                (int)(Item.damage * dmgMult),
                knockback,
                player.whoAmI
            );
            var p = Main.projectile[proj];
            p.hostile = false;   // ⬅ 把敌对关掉
            p.friendly = true;   // ⬅ 设为友方
            p.owner = player.whoAmI;   // ⬅ 设为自己
            // 调整尺寸：对“会读取 scale 绘制/判定”的弹幕有效。HarpyFeather 会跟随 scale 绘制，但命中盒不一定等比；
            // 如需更严谨，建议自定义 ModProjectile，在其中用 scale 或 AI 参数控制判定。
            p.scale *= scaleMult;
            p.light = 0.1f;
            return false; // 阻止默认发射（因为我们已经手动发射了）
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HarpyWings, 2)
                .AddIngredient(ItemID.Cloud, 10)
                .AddIngredient(ItemID.EnchantedSword, 1)
                .AddTile(TileID.SkyMill);
        }
    }
}