using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;

namespace WuDao.Content.Projectiles.Summon;

public class FishSchoolWhipProj : ModProjectile
{
    public override string Texture => "WuDao/Content/Projectiles/Summon/FlyingSnakeWhipProjectile";

    // 任务鱼生成间隔（同一个目标）
    private const int FishSpawnCooldown = 24; // 24 tick = 0.4 秒
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.IsAWhip[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.DefaultToWhip();
        Projectile.WhipSettings.Segments = 26;
        Projectile.WhipSettings.RangeMultiplier = 1.3f;
    }
    // 目前冷却时对武器而言的，武器打A后马上打B也会被冷却住
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // ai[0] 作为该鞭子对每个目标的本地命中冷却计时入口不合适，
        // 最简单的方法：直接使用 target 的 immunity 概念不太适配这个需求，
        // 所以推荐配一个 GlobalNPC 存 timer。
        // 如果你想先快速实现，这里用 localAI[0] 做“当前鞭子实例的额外射弹冷却”也行。

        if (Projectile.localAI[0] > 0f)
            return;

        Projectile.localAI[0] = FishSpawnCooldown;

        Player player = Main.player[Projectile.owner];

        // int fishCount = Main.rand.Next(2, 5);

        // for (int i = 0; i < fishCount; i++)
        {
            bool spawnOnLeft = Main.rand.NextBool();

            // 以“正下方”为基准，偏移 30°~60°
            float offsetDeg = Main.rand.NextFloat(30f, 60f);
            float offsetRad = MathHelper.ToRadians(offsetDeg);

            // 出生半径
            float radius = Main.rand.NextFloat(90f, 180f);

            Vector2 spawnOffset;

            if (spawnOnLeft)
            {
                // 敌怪正下方往左偏 30~60°
                spawnOffset = Vector2.UnitY.RotatedBy(-offsetRad) * radius;
            }
            else
            {
                // 敌怪正下方往右偏 30~60°
                spawnOffset = Vector2.UnitY.RotatedBy(offsetRad) * radius;
            }

            Vector2 spawnPos = target.Center + spawnOffset;

            Vector2 launchVelocity = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.Next(9, 14);

            int fishItem = ItemSets.TaskFishSet.Get(SelectionMode.Random);

            // 生成任务鱼
            int proj = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos,
                launchVelocity,
                ModContent.ProjectileType<QuestFishProjectile>(),
                (int)(damageDone * 0.8f),
                0f,
                player.whoAmI,
                fishItem
            );
        }
    }

    public override void AI()
    {
        if (Projectile.localAI[0] > 0f)
            Projectile.localAI[0]--;
    }
}