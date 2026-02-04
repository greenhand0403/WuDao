using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Projectiles.Melee;
using System.Collections.Generic;

namespace WuDao.Content.Items.Weapons.Melee
{
    // 海蓝军刀
    public class OceanBlueSword : BuffItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ModContent.ItemType<HellfireSword>());
            Item.shoot = ModContent.ProjectileType<OceanBlueSwordProjectile>();
        }
        // ① 手持时给：水下呼吸（Gills）+ 游泳（Flipper）+ 水上漂（WaterWalking）
        protected override void BuildBuffRules(Player player, Item item, IList<BuffRule> rules)
        {
            rules.Add(new BuffRule(BuffConditions.Always,
                BuffEffect.PermanentBuff(BuffID.Gills), // ← 永久/不减时/无时间显示
                BuffEffect.PermanentBuff(BuffID.Flipper),
                BuffEffect.PermanentBuff(BuffID.WaterWalking)
            ));
        }
        // ② 手持时免疫：霜冻（Frostburn）与 冰冻（Frozen）
        //    （如需也免疫“寒冷(Chilled)”，把 BuffID.Chilled 也加进去）
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            rules.Add(new StatRule(BuffConditions.Always,
                StatEffect.ImmuneTo(BuffID.Frostburn, BuffID.Frozen),
                StatEffect.ImmuneTo(BuffID.Chilled) // 可选：免疫“寒冷”
            ));
        }
        // ③ 武器本体命中：25% 几率施加“霜冻”（2 秒 = 120 帧）
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.25f)
                target.AddBuff(BuffID.Frostburn, 120);
        }
        // ④ 海洋环境增伤：在沙滩/海边（ZoneBeach）时 +20% 最终伤害
        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (player.ZoneBeach)
                modifiers.FinalDamage *= 1.20f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HellfireSword>(), 1) // 使用地狱之锋作为材料
                .AddCondition(Condition.NearWater)   // 必须在水边才能合成
                .Register();
        }
    }
}
