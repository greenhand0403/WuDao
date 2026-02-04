using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Items;
using WuDao.Content.Global; // ArmorElementTag

namespace WuDao.Content.Players
{
    // 五行盔甲系统
    public class ArmorElementPlayer : ModPlayer
    {
        private bool goldFullSet; // 金三件：护甲穿透
        private bool fireFullSet; // 火三件：暴击伤害

        public override void ResetEffects()
        {
            goldFullSet = false;
            fireFullSet = false;
        }

        public override void UpdateEquips()
        {
            int gold = 0, wood = 0, water = 0, fire = 0, earth = 0;

            Item[] armor = { Player.armor[0], Player.armor[1], Player.armor[2] };
            foreach (var item in armor)
            {
                if (item == null || item.IsAir) continue;

                var tag = item.GetGlobalItem<ArmorElementTag>();
                int e = tag?.Element ?? -1;
                if (e == 0) gold++;
                else if (e == 1) wood++;
                else if (e == 2) water++;
                else if (e == 3) fire++;
                else if (e == 4) earth++;
            }

            // —— 金 —— 每件 +3% 伤害；三件额外 +3% 伤害；护甲穿透 +6（命中时加）
            if (gold > 0)
            {
                Player.GetDamage(DamageClass.Generic) += 0.03f * gold;
                if (gold == 3)
                {
                    Player.GetDamage(DamageClass.Generic) += 0.03f;
                    goldFullSet = true;
                }
            }

            // —— 木 —— 每件 +3 生命再生；三件额外 +3 与 生命上限 +30%
            if (wood > 0)
            {
                Player.lifeRegen += 3 * wood;
                if (wood == 3)
                {
                    Player.lifeRegen += 3;
                    Player.statLifeMax2 = (int)(Player.statLifeMax2 * 1.3f);
                }
            }

            // —— 水 —— 每件 +4% 移速；三件额外 +4% 且加/减速 +10%
            if (water > 0)
            {
                Player.moveSpeed += 0.04f * water;
                if (water == 3)
                {
                    Player.moveSpeed += 0.04f;
                    Player.runAcceleration *= 1.1f;
                    // 若你的版本没有 runSlowdown 字段，注释下一行即可
                    Player.runSlowdown *= 1.1f;
                }
            }

            // —— 火 —— 每件 +2% 暴击率；三件额外 +2% 且暴击伤害 +10%（命中时加）
            if (fire > 0)
            {
                Player.GetCritChance(DamageClass.Generic) += 2 * fire;
                if (fire == 3)
                {
                    Player.GetCritChance(DamageClass.Generic) += 2;
                    fireFullSet = true;
                }
            }

            // —— 土 —— 每件 +3 防御；三件额外 +3 防御 且耐力减伤 +10%
            if (earth > 0)
            {
                Player.statDefense += 3 * earth;
                if (earth == 3)
                {
                    Player.statDefense += 3;
                    Player.endurance += 0.10f;
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            ApplyOnHitBonuses(ref modifiers);
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            ApplyOnHitBonuses(ref modifiers);
        }

        private void ApplyOnHitBonuses(ref NPC.HitModifiers modifiers)
        {
            if (goldFullSet) modifiers.ArmorPenetration += 6;
            if (fireFullSet) modifiers.CritDamage += 0.10f;
        }
    }
}
