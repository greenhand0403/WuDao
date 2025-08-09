using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Cooldowns;

// This file is part of the Wu Dao mod for Terraria.
// It defines the Steel Heavy Sword item, which is a weapon in the game.
namespace WuDao.Content.Items.Weapons.Melee
{
    public class SteelHeavySword : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40; // Width of the item
            Item.height = 40;

            Item.useStyle = ItemUseStyleID.Swing; // Style of use
            Item.useAnimation = 30; // Animation duration
            Item.useTime = 30; // Time taken to use the item
            Item.autoReuse = true; // Allows the item to be used repeatedly without clicking again

            Item.damage = 18; // Damage dealt by the sword
            Item.DamageType = DamageClass.Melee; // Type of damage
            Item.knockBack = 7f; // Knockback effect
            Item.crit = 8; // Critical hit chance

            Item.rare = ItemRarityID.Green; // Rarity of the item
            Item.value = Item.buyPrice(silver: 4); // Value of the item in gold
            Item.UseSound = SoundID.Item1; // Sound played when the item is used

            Item.useTurn = true; // Allows the player to turn while using the item
            Item.noUseGraphic = false; // Shows the item's graphic when used
            Item.noMelee = false;
        }
        public override bool AltFunctionUse(Player player) => true; // Allows the item to be used with an alternate function (like a special attack)
        public override void ModifyItemScale(Player player, ref float scale)
        {
            int cd = player.GetModPlayer<SteelHeavySwordPlayer>().RightClickCooldown;

            // 只在 90~120 的窗口内做分段缩放
            const int start = 90;
            const int end = 120;
            const int window = end - start; // 30

            if (cd > start && cd <= end)
            {
                // 归一化时间 t ∈ [0,1]，t=0 对应 cd=120，t=1 对应 cd=90
                float t = MathHelper.Clamp((end - cd) / (float)window, 0f, 1f);

                float factor;
                if (t < 0.5f)
                {
                    // 前 50%：0.8 -> 2.0
                    float p = t / 0.5f; // ∈[0,1]
                    factor = MathHelper.Lerp(1.0f, 2.0f, p);
                }
                else if (t < 0.7f)
                {
                    // 中间 20%：保持 2.0
                    factor = 2.0f;
                }
                else
                {
                    // 最后 30%：2.0 -> 1.0
                    float p = (t - 0.7f) / 0.3f; // ∈[0,1]
                    factor = MathHelper.Lerp(2.0f, 1.0f, p);
                }

                scale *= factor;
            }
            // 其它时间段不改 scale（保持物品原始尺寸）
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // 右键时至少挥舞了1/2的动画，才判定碰撞
            // if (!noHitbox && (player.altFunctionUse == 2) && ((Item.useAnimation - player.itemAnimation) > (Item.useAnimation / 2)))
            // {
            //     noHitbox = true;
            //     return;
            // }
            // 90~120是右击挥舞动画，110~120期间不碰撞，确保至少挥出1/3的动画
            if (!noHitbox && (player.altFunctionUse == 2) && (player.GetModPlayer<SteelHeavySwordPlayer>().RightClickCooldown > 110))
            {
                noHitbox = true;
                return;
            }
        }
        public override bool CanUseItem(Player player)
        {
            if (player.GetModPlayer<SteelHeavySwordPlayer>().RightClickCooldown > 0) // Check if the cooldown is over
            {
                return false; // Prevent using the item if the cooldown is not over
            }
            if (player.altFunctionUse == 2) // Check if the alternate function is being used
            {
                Item.damage = 30;
                Item.knockBack = 10f; // Increase knockback for the alternate use
                Item.UseSound = SoundID.Item71; // Use the same sound for the alternate use
                player.GetModPlayer<SteelHeavySwordPlayer>().RightClickCooldown = 120; // 2秒冷却 (60 tick/s)
            }
            else
            {
                Item.damage = 18; // Reset to normal damage
                Item.knockBack = 7f; // Reset to normal knockback
                Item.UseSound = SoundID.Item1; // Use the same sound for the normal use
            }
            return base.CanUseItem(player); // Allow the item to be used
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(5))
            {
                // Create a dust effect when the sword hits something
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.SilverFlame);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode == NetmodeID.Server) return;    // 服务器不生成粒子

            // 判断是否是右键攻击
            if (player.altFunctionUse == 2)
            {
                Rectangle spawnRect = new Rectangle(
                    target.Hitbox.Left,
                    target.Hitbox.Top + 6,   // 稍微在顶部之上
                    target.Hitbox.Width,
                    6
                );

                for (int i = 0; i < 30; i++)
                {
                    int idx = Dust.NewDust(spawnRect.Location.ToVector2(), spawnRect.Width, spawnRect.Height,
                                           Main.rand.Next(DustID.Dirt, DustID.Stone), 0f, 0f, 150, default, Main.rand.NextFloat(0.8f, 1.8f));

                    Dust d = Main.dust[idx];
                    // 向上喷：Y 负值；X 做点随机横向散射
                    d.velocity = new Vector2(Main.rand.NextFloat(-2.2f, 2.2f), Main.rand.NextFloat(-6f, -3f));
                    d.noGravity = true; // 想让它飞一下再落下，可以混合几颗有重力的：
                    if (Main.rand.NextBool(3)) d.noGravity = false;
                }
                // 播放重击音效
                SoundEngine.PlaySound(SoundID.Item71, target.Center);
                if (Main.rand.NextBool(2))
                {
                    target.AddBuff(BuffID.Bleeding, 120); // 2秒流血
                }
                player.SetItemAnimation(0);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 2)
                .AddIngredient(ItemID.CopperBar, 2)
                .AddIngredient(ItemID.IronBar, 2) // Requires 2 Copper Bars and 2 Iron Bars
                .AddTile(TileID.Anvils) // Crafted at an Anvil
                .Register(); // Register the recipe
        }
    }
}