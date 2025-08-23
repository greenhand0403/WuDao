using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Melee;
using WuDao.Content.Cooldowns;

namespace WuDao.Content.Items.Weapons.Melee
{
    // TODO: 重绘贴图
    public class GhostChakram : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName / Tooltip 按需加本地化
        }
        public override string Texture => $"Terraria/Images/Item_{ItemID.LightDisc}";
        public override void SetDefaults()
        {
            Item.damage = 48;
            Item.DamageType = DamageClass.MeleeNoSpeed; // 近战投掷类（不吃攻速）
            Item.knockBack = 4.5f;

            Item.width = 40;
            Item.height = 40;

            Item.useStyle = ItemUseStyleID.Swing; // 丢掷手感
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.autoReuse = true;
            Item.noUseGraphic = true;   // 投射时不显示物品
            Item.noMelee = true;
            Item.maxStack = 3;
            Item.shoot = ModContent.ProjectileType<GhostChakramProj>();
            Item.shootSpeed = 14f;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        // 限制场上同类回旋镖数量（要像光辉飞盘那样允许多枚可移除此限制）
        public override bool CanUseItem(Player player)
        {
            int ptype = ModContent.ProjectileType<GhostChakramProj>();
            return player.ownedProjectileCounts[ptype] < Item.stack;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.whoAmI != Main.myPlayer)
                return false; // 只在本地生成，防止联机重复

            var mp = player.GetModPlayer<GhostChakramPlayer>();

            // 推进循环：0,1 普通；到 2 时 -> 强化并回到 0
            bool empowered = false;
            if (mp.GhostChakramCycle == 2)
            {
                empowered = true;
                mp.GhostChakramCycle = 0;
            }
            else
            {
                mp.GhostChakramCycle++;
            }

            int finalDamage = damage;
            float ai1 = 0f;
            if (empowered)
            {
                finalDamage = (int)(damage * 1.2f); // 120% 伤害
                ai1 = 1f;                           // 强化标记，给 Proj 用
            }

            int proj = Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                position, velocity, type, finalDamage, knockback, player.whoAmI,
                ai0: 0f, ai1: ai1);

            if (proj >= 0 && proj < Main.maxProjectiles)
                Main.projectile[proj].netUpdate = true;

            return false; // 我们手动生成
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Ectoplasm, 10); // 自定义掉落
            recipe.AddIngredient(ItemID.SpectreBar, 8);                     // 幽灵锭
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
