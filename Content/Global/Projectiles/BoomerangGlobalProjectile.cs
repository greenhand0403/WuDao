using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Global.Projectiles
{
    // 三个回旋镖饰品：穿墙和返程效果
    public class BoomerangGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        private float initialSpeed;
        private int suppressReturnTicks; // 命中后短暂抑制“因命中折返”
        private Vector2 cachedForwardVel;

        // —— 可调参数 —— 
        private const int SuppressFrames = 3;         // 抑制帧数（2~4都行）

        private bool inReturnPhase;        // —— 新增：回程阶段“锁存”状态（解决只翻倍一次）
        private int lastAi0;               // —— 新增：监控 ai[0] 的跃迁
        private static bool HasProjectileAuthority(Projectile projectile)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return true;

            if (Main.netMode == NetmodeID.Server)
                return true;

            return projectile.owner == Main.myPlayer;
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (IsBoomerang(projectile))
            {
                initialSpeed = projectile.velocity.Length();
                inReturnPhase = false;
                lastAi0 = 0;
            }
        }

        public override void PostAI(Projectile projectile)
        {
            if (!projectile.active || !projectile.friendly || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            if (!IsBoomerang(projectile))
                return;

            Player owner = Main.player[projectile.owner];
            if (owner == null || !owner.active)
                return;

            if (!HasProjectileAuthority(projectile))
                return;

            var mp = owner.GetModPlayer<BoomerangAccessoryPlayer>();
            bool changed = false;

            // —— 跃升：穿墙 + 无限穿透（只在状态变化时同步一次）
            if (mp.Yuesheng)
            {
                if (projectile.tileCollide)
                {
                    projectile.tileCollide = false;
                    changed = true;
                }

                if (projectile.penetrate != -1)
                {
                    projectile.penetrate = -1;
                    changed = true;
                }

                if (!projectile.usesLocalNPCImmunity)
                {
                    projectile.usesLocalNPCImmunity = true;
                    changed = true;
                }

                if (projectile.localNPCHitCooldown < 8)
                {
                    projectile.localNPCHitCooldown = 8;
                    changed = true;
                }
            }

            if (changed)
                projectile.netUpdate = true;

            // —— 抑制窗口：把“因命中导致的回程”打回外放
            if (mp.Yuesheng && suppressReturnTicks > 0)
            {
                if (projectile.ai[0] != 0f)
                {
                    projectile.ai[0] = 0f;
                    if (cachedForwardVel != Vector2.Zero)
                        projectile.velocity = cachedForwardVel;
                    projectile.netUpdate = true;
                }
                suppressReturnTicks--;
            }

            // === 回程阶段“锁存”判定 ===
            {
                int ai0 = (int)projectile.ai[0];

                // 从外放(0) -> 回程(非0) 且不在抑制期：进入真正回程
                if (lastAi0 == 0 && ai0 != 0 && suppressReturnTicks <= 0)
                {
                    inReturnPhase = true;
                }
                lastAi0 = ai0;
            }

            // === 归心似箭：回程加速 + 收镖 ===
            if (mp.Guixin && inReturnPhase)
            {
                Vector2 toOwner = owner.MountedCenter - projectile.Center;
                float dist = toOwner.Length();
                Vector2 dir = dist > 1f ? toOwner / dist : Vector2.Zero;

                float baseSpeed = initialSpeed > 0.01f ? initialSpeed : projectile.velocity.Length();
                float maxDesired = baseSpeed * 2f;
                float nearLimited = MathF.Min(maxDesired, MathF.Max(6f, dist * 0.5f));

                Vector2 targetVel = dir * nearLimited;
                projectile.velocity = Vector2.Lerp(projectile.velocity, targetVel, 0.5f);

                const float catchRadius = 18f;
                if (dist <= catchRadius)
                {
                    SafeCatch(projectile);
                    return;
                }

                Vector2 nextPos = projectile.Center + projectile.velocity;
                float dNow = dist;
                float dNext = (owner.MountedCenter - nextPos).Length();

                if (Vector2.Dot(projectile.velocity, toOwner) < 0f || dNext > dNow + 2f)
                {
                    SafeCatch(projectile);
                    return;
                }
            }

            // 非归心似箭但已在回程阶段，也加一个保底接取
            if (inReturnPhase)
            {
                float dist = Vector2.Distance(owner.MountedCenter, projectile.Center);
                if (dist <= 14f)
                {
                    SafeCatch(projectile);
                    return;
                }
            }
        }

        // —— 安全接取：避免残留状态
        private void SafeCatch(Projectile p)
        {
            inReturnPhase = false;
            p.netUpdate = true;
            p.Kill();
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!IsBoomerang(projectile))
                return;

            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            if (!HasProjectileAuthority(projectile))
                return;

            Player owner = Main.player[projectile.owner];
            if (owner == null || !owner.active)
                return;

            var mp = owner.GetModPlayer<BoomerangAccessoryPlayer>();

            if (mp.Yuesheng)
            {
                // 命中后短暂抑制“因命中导致的折返”
                cachedForwardVel = projectile.oldVelocity;
                if (cachedForwardVel.Length() < 0.01f)
                    cachedForwardVel = projectile.velocity;

                suppressReturnTicks = SuppressFrames;
            }
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            // 真实伤害结果：多人里只让服务器判，避免客户端重复参与
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (!IsBoomerang(projectile))
                return;

            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            Player owner = Main.player[projectile.owner];
            if (owner == null || !owner.active)
                return;

            var mp = owner.GetModPlayer<BoomerangAccessoryPlayer>();

            // 一旦进入回程阶段，回程途中每一击都翻倍
            if (mp.Yanfan && inReturnPhase)
                modifiers.SourceDamage *= 2f;
        }

        /// <summary>是否是原版回旋镖AI</summary>
        private static bool IsBoomerang(Projectile p)
        {
            return p.aiStyle == ProjAIStyleID.Boomerang;
        }
    }
}