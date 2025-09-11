// 九阴白骨爪（被动）：发射射弹时，消耗10气在鼠标处生成“暗影爪”投射物。
// 贴图占位：ShadowBeamFriendly（后续可换成暗影之爪自绘弹幕）。
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Juexue.Base;
using System.Collections.Generic;

namespace WuDao.Content.Juexue.Passive
{
    // TODO: 爪虚影
    public class WhiteBoneClaw : JuexueItem
    {
        public override bool IsActive => false;
        public const int Cost = 10;
        public const float Chance = 0.35f; // 可按需调整：被动触发率

        // 九阴白骨爪（被动）：复刻原版“暗影爪(964)”生成逻辑
        public void TryPassiveTriggerOnShoot(Player player, QiPlayer qi, EntitySource_ItemUse_WithAmmo src,
            Vector2 pos, Vector2 vel, int type, int dmg, float kb)
        {
            if (qi.QiMax <= 0) return;
            if (Main.rand.NextFloat() > Chance) return;
            if (!qi.TrySpendQi(Cost)) return;

            // —— 复刻 SpawnHallucination 的目标筛选 —— //
            const float range = 500f;
            var candidates = new List<NPC>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.CanBeChasedBy(player) &&
                    Vector2.Distance(player.Center, n.Center) <= range &&
                    Collision.CanHitLine(player.position, player.width, player.height, n.position, n.width, n.height))
                {
                    candidates.Add(n);
                }
            }
            if (candidates.Count == 0) return;

            // —— 关键：用 RandomizeInsanityShadowFor 生成位置/速度/AI —— //
            NPC target = Main.rand.NextFromCollection(candidates);
            Projectile.RandomizeInsanityShadowFor(target, isHostile: false,
                out Vector2 spawnPos, out Vector2 spawnVel, out float ai0, out float ai1);

            // —— 生成原版 964 号弹幕（友方暗影爪） —— //
            // 等价于 ProjectileID.InsanityShadowFriendly；为避免常量差异，这里直接写 964 也可
            int projType = ProjectileID.InsanityShadowFriendly; // == 964
            int damage = 18;        // 原版固定伤害
            float knockBack = 0f;   // 原版就是 0
            Projectile.NewProjectile(src, spawnPos, spawnVel, projType, damage, knockBack, player.whoAmI, ai0, ai1);
        }
    }
}
