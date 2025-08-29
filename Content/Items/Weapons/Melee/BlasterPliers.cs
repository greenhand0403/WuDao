using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Weapons.Melee
{
    class BlasterPliers : ModItem
    {
        private int foodCount;
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.CombatWrench);
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.damage = 30;
            Item.noMelee = false;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shoot = ProjectileID.None;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
            Item.width = 48;
            Item.height = 48;
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // 按“已制作 X 种可合成食物”每种 +50 伤害做平铺加成；按“已品尝 Y 种”每种 +10% 乘法
            // cmd Main.NewText($"已制作={madeCount}, 已品尝={eatenCount}");
            var cp = player.GetModPlayer<CuisinePlayer>();
            // 根据厨艺值加成伤害
            int madeCount = (int)(cp.CookingSkill * 0.01f);
            damage.Flat += madeCount;

            damage.Flat += foodCount;
        }
        public override float UseSpeedMultiplier(Player player)
        {
            var cp = player.GetModPlayer<CuisinePlayer>();
            // 根据美味值加成攻速
            return 1f + cp.Deliciousness * 0.01f;
        }
        // public override void UpdateAccessory(Player player, bool hideVisual)
        // {
        //     var cp = player.GetModPlayer<CuisinePlayer>();
        //     int madeCount = cp.CraftedFoodTypes.Count;  // 已制作过“可合成食物”的种数（走菜谱池）
        //     int eatenCount = cp.FoodsEatenAll.Count;     // 全局“已吃过”的种数（含不可合成与模组）

        //     // 举例：根据“厨艺/制作数”给少量加成
        //     if (madeCount >= 5) player.GetDamage(DamageClass.Generic) += 0.05f;

        //     // 控制台/聊天栏直接打印
        //     Main.NewText($"Cookbook已制作：{madeCount}，FoodLog已品尝：{eatenCount}");
        // }
        public override void UpdateEquip(Player player)
        {
            foodCount = 0;
            for (int i = 0; i < 50; i++)
            {
                if (player.inventory[i].type == ItemID.Hotdog)
                {
                    foodCount+=player.inventory[i].stack;
                }
            }
        }
        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            int g = target.rarity;
            if (g > 0)
            {
                float multiplier = 1f + g * 0.5f;
                modifiers.FinalDamage *= multiplier;

                CombatText.NewText(
                    target.Hitbox,
                    new Color(180, 220, 255),
                    $"敌怪稀有度 {g}"
                );
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.OldShoe)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}