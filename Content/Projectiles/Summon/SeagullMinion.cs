using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles.Summon;

public class SeagullMinion : ModProjectile
{
    private enum SeagullAnimState
    {
        Flying = 0,
        Perched = 1,
        Resting = 2
    }

    private int poopTimer;
    private int perchStableTimer;

    public override string Texture => "Terraria/Images/NPC_" + NPCID.Seagull;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 15;
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = true;
        ProjectileID.Sets.CultistIsResistantTo[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 36;
        Projectile.height = 30;

        Projectile.minion = true;
        Projectile.minionSlots = 1f;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 18000;
        Projectile.netImportant = true;
    }

    public override bool? CanCutTiles() => false;

    public override bool MinionContactDamage() => false;

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        if (!player.active || player.dead)
        {
            player.ClearBuff(ModContent.BuffType<SeagullBuff>());
            return;
        }

        if (player.HasBuff(ModContent.BuffType<SeagullBuff>()))
        {
            Projectile.timeLeft = 2;
        }

        Vector2 toPlayer = player.Center - Projectile.Center;
        if (toPlayer.Length() > 1800f)
        {
            Projectile.Center = player.Center + new Vector2(0f, -80f);
            Projectile.velocity *= 0.2f;
            Projectile.netUpdate = true;
        }

        NPC target = FindTarget(player);

        if (target != null)
            AttackAI(player, target);
        else
            IdleAI(player);

        if (Math.Abs(Projectile.velocity.X) > 0.15f)
            Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X > 0f ? -1 : 1;
    }

    private NPC FindTarget(Player player)
    {
        NPC target = null;
        float maxDist = 900f;

        if (player.HasMinionAttackTargetNPC)
        {
            NPC forcedTarget = Main.npc[player.MinionAttackTargetNPC];
            if (forcedTarget.CanBeChasedBy() && Vector2.Distance(forcedTarget.Center, Projectile.Center) <= 1200f)
                return forcedTarget;
        }

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy())
                continue;

            float dist = Vector2.Distance(npc.Center, Projectile.Center);
            if (dist < maxDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
            {
                maxDist = dist;
                target = npc;
            }
        }

        return target;
    }

    private void AttackAI(Player player, NPC target)
    {
        perchStableTimer = 0;

        // 围绕“敌人上方”做横向盘旋
        float orbitTime = Main.GameUpdateCount * 0.08f + Projectile.whoAmI * 0.7f;
        Vector2 orbitCenter = target.Center + new Vector2(0f, -120f);

        float horizontalRadius = 64f;
        float verticalBob = 12f;

        Vector2 desiredPos = orbitCenter + new Vector2(
            (float)Math.Sin(orbitTime) * horizontalRadius,
            (float)Math.Cos(orbitTime * 2f) * verticalBob
        );

        Vector2 toDesired = desiredPos - Projectile.Center;
        float inertia = 20f;
        float speed = 14f;

        Vector2 desiredVelocity = toDesired.SafeNormalize(Vector2.Zero) * speed;
        Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desiredVelocity) / inertia;

        UpdateAnimation(SeagullAnimState.Flying);

        poopTimer++;
        if (poopTimer >= 45)
        {
            poopTimer = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 spawnPos = Projectile.Center + new Vector2(0f, 8f);
                // 往敌怪身上投射 poop
                Vector2 poopVelocity = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.Next(8, 10);

                int proj = Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    spawnPos,
                    poopVelocity,
                    ModContent.ProjectileType<PoopProjectile>(),
                    Projectile.damage,
                    0f,
                    Projectile.owner
                );
                if (proj > 0)
                {
                    Main.projectile[proj].netUpdate = true;
                }
            }
        }
    }

    private void IdleAI(Player player)
    {
        poopTimer = 0;

        bool playerStill = player.velocity.Length() <= 0.15f;

        // 常规待机跟随点
        Vector2 followPos = player.Center + new Vector2(player.direction * 56f, -72f);

        // 玩家静止时，尝试找一个落脚点
        if (playerStill && TryFindPerchSpot(player, out Vector2 perchPos))
        {
            Vector2 toPerch = perchPos - Projectile.Center;
            float dist = toPerch.Length();

            if (dist > 10f)
            {
                // 飞向落脚点
                Vector2 desiredVelocity = toPerch.SafeNormalize(Vector2.Zero) * MathHelper.Clamp(dist * 0.12f, 2f, 10f);
                Projectile.velocity = (Projectile.velocity * 24f + desiredVelocity) / 25f;
                UpdateAnimation(SeagullAnimState.Flying);
            }
            else
            {
                // 已到落脚点
                Projectile.Center = Vector2.Lerp(Projectile.Center, perchPos, 0.25f);
                Projectile.velocity *= 0.75f;

                perchStableTimer++;

                if (perchStableTimer <= 18)
                    UpdateAnimation(SeagullAnimState.Perched); // 先播站立
                else
                    UpdateAnimation(SeagullAnimState.Resting); // 然后进入 2、3 帧循环
            }

            return;
        }

        // 玩家移动时正常跟随
        perchStableTimer = 0;
        Vector2 toFollow = followPos - Projectile.Center;
        float followDist = toFollow.Length();

        if (followDist > 220f)
        {
            Vector2 desiredVelocity = toFollow.SafeNormalize(Vector2.Zero) * 12f;
            Projectile.velocity = (Projectile.velocity * 19f + desiredVelocity) / 20f;
        }
        else
        {
            Projectile.velocity *= 0.94f;
            Projectile.velocity += toFollow * 0.0035f;
        }

        UpdateAnimation(SeagullAnimState.Flying);
    }

    private bool TryFindPerchSpot(Player player, out Vector2 perchPos)
    {
        Point baseTile = player.Bottom.ToTileCoordinates();

        // 在玩家附近搜索平台/物块顶面
        for (int dx = -8; dx <= 8; dx++)
        {
            for (int dy = -2; dy <= 6; dy++)
            {
                int x = baseTile.X + dx;
                int y = baseTile.Y + dy;

                if (x < 10 || x >= Main.maxTilesX - 10 || y < 10 || y >= Main.maxTilesY - 10)
                    continue;

                Tile tile = Main.tile[x, y];
                Tile above = Main.tile[x, y - 1];

                if (tile == null || above == null)
                    continue;

                bool solidTop = tile.HasTile && (Main.tileSolid[tile.TileType] || TileID.Sets.Platforms[tile.TileType]);
                bool airAbove = !above.HasTile || !Main.tileSolid[above.TileType];

                if (solidTop && airAbove)
                {
                    // 找到平台/物块顶面，设置落脚点
                    perchPos = new Vector2(x * 16f + 8f, y * 16f - 16f);
                    return true;
                }
            }
        }

        perchPos = Vector2.Zero;
        return false;
    }

    private void UpdateAnimation(SeagullAnimState state)
    {
        Projectile.frameCounter++;

        switch (state)
        {
            case SeagullAnimState.Flying:
                // 最后4帧：11~14
                if (Projectile.frame < 11 || Projectile.frame > 14)
                    Projectile.frame = 11;

                if (Projectile.frameCounter >= 5)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame > 14)
                        Projectile.frame = 11;
                }
                break;

            case SeagullAnimState.Perched:
                // 第1帧：0
                Projectile.frame = 0;
                Projectile.frameCounter = 0;
                break;

            case SeagullAnimState.Resting:
                // 第2、3帧：1~2
                if (Projectile.frame < 1 || Projectile.frame > 2)
                    Projectile.frame = 1;

                if (Projectile.frameCounter >= 14)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame > 2)
                        Projectile.frame = 1;
                }
                break;
        }
    }
}