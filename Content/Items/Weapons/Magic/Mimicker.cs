using System;
using System.Linq;
using Terraria.Localization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Systems;
using WuDao.Content.Global.Projectiles;

namespace WuDao.Content.Items.Weapons.Magic
{
    // =====================
    // 2) 模仿者：Mimicker
    // - 初始仅能发射宝石法杖类射弹
    // - 用其射弹击败特定远程系敌怪，累计达到阈值后，解锁该敌怪对应“可用射弹”加入随机池
    // - 伤害随解锁种类数提升
    // =====================
    public class Mimicker : ModItem
    {
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(90, 4));
        }
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(gold: 5);

            Item.DamageType = DamageClass.Magic;
            Item.damage = 22; // 初始略低，靠收集叠加
            Item.knockBack = 2f;
            Item.mana = 5;

            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item85;
            Item.noMelee = true;

            Item.shoot = ProjectileID.WoodenArrowFriendly; // 占位
            Item.shootSpeed = 10.5f;
            Item.autoReuse = true;
        }

        // 伤害随“已收集的射弹种类数”线性提升
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            var mp = player.GetModPlayer<MimickerPlayer>();
            int unlocked = mp.unlockedProjectiles.Count;
            // 每解锁1种 +10%（可自行调整）
            damage += 0.1f * unlocked;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var mp = player.GetModPlayer<MimickerPlayer>();
            // 构建当前可用随机池（基础+解锁）
            var pool = new List<int>(mp.BuildCurrentPool());
            int choice = pool[Main.rand.Next(pool.Count)];

            Vector2 perturbed = velocity.RotatedByRandom(MathHelper.ToRadians(5));
            float speedScale = 0.95f + Main.rand.NextFloat(0.15f);
            int p = Projectile.NewProjectile(source, position, perturbed * speedScale, choice, damage, knockback, player.whoAmI);
            if (p >= 0 && p < Main.maxProjectiles)
            {
                Main.projectile[p].GetGlobalProjectile<MimickerGlobalProjectile>().fromMimicker = true;
            }
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer is Player p)
            {
                var mp = p.GetModPlayer<MimickerPlayer>();
                int baseInit = MimickerSystem.UnlockTable.Length;
                int unlocked = mp.unlockedProjectiles.Count;
                int remainBase = Math.Max(0, baseInit - unlocked);
                int totalPool = MimickerSystem.BasePool.Length;

                // 汇总信息
                tooltips.Add(new TooltipLine(Mod, "MimickerInfo",
                    $"随机池：{totalPool}（基础剩余 {remainBase}/{baseInit}，已解锁 {unlocked}）"));

                // 逐条进度（仅显示“尚未解锁”的目标）
                int lines = 0;
                const int MaxLines = 8; // 防止过长，你可调大/去掉限制

                foreach (var def in MimickerSystem.UnlockTable)
                {
                    // 已经解锁的就不展示剩余
                    if (mp.unlockedProjectiles.Contains(def.ProjectileType))
                        continue;

                    int cur = mp.killProgress.TryGetValue(def.NpcType, out int cnt) ? cnt : 0;
                    int remain = Math.Max(0, def.Required - cur);

                    // 只展示还需要击杀的目标
                    if (remain > 0)
                    {
                        string npcName = Lang.GetNPCNameValue(def.NpcType);
                        string projName = Lang.GetProjectileName(def.ProjectileType).Value;

                        tooltips.Add(new TooltipLine(Mod, "MimickerProgress",
                            $"击败 {npcName} 还需 {remain} 次 → 解锁 {projName}"));

                        lines++;
                        if (lines >= MaxLines) break;
                    }
                }

                // 可选：若还有更多未显示的目标
                int hidden = MimickerSystem.UnlockTable.Count(d => !mp.unlockedProjectiles.Contains(d.ProjectileType) &&
                    (d.Required - (mp.killProgress.TryGetValue(d.NpcType, out int c) ? c : 0)) > 0) - lines;

                if (hidden > 0)
                {
                    tooltips.Add(new TooltipLine(Mod, "MimickerMore",
                        $"…还有 {hidden} 项未显示的解锁进度"));
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Bone, 25)
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddIngredient(ItemID.CrystalShard, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}