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
        private const float MaxReturnSpeedMul = 2f;   // 归心似箭倍率（外放初速*2）
        private const float MinCatchRadius = 18f;     // 接住半径（像素）
        private const float ApproachSoftness = 0.5f;  // Lerp系数，越大越灵敏（0.35~0.6）
        private const float NearSpeedFloor = 6f;      // 距离很近时至少保持的速度

        private bool inReturnPhase;        // —— 新增：回程阶段“锁存”状态（解决只翻倍一次）
        private int lastAi0;               // —— 新增：监控 ai[0] 的跃迁


        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (IsBoomerang(projectile))
                initialSpeed = projectile.velocity.Length();

            inReturnPhase = false;
            lastAi0 = 0;
        }

        public override void PostAI(Projectile projectile)
        {
            if (!IsBoomerang(projectile)) return;

            Player owner = Main.player[projectile.owner];
            var mp = owner.GetModPlayer<BoomerangAccessoryPlayer>();

            // —— 跃升：穿墙 + 无限穿透（保持你的写法）
            if (mp.Yuesheng)
            {
                projectile.tileCollide = false;
                projectile.penetrate = -1;
                projectile.usesLocalNPCImmunity = true;
                if (projectile.localNPCHitCooldown < 8)
                    projectile.localNPCHitCooldown = 8;
            }

            // —— 抑制窗口：把“因命中导致的回程”打回外放（保持你的写法）
            if (mp.Yuesheng && suppressReturnTicks > 0)
            {
                if (projectile.aiStyle == ProjAIStyleID.Boomerang && projectile.ai[0] != 0f)
                {
                    projectile.ai[0] = 0f;
                    if (cachedForwardVel != Vector2.Zero)
                        projectile.velocity = cachedForwardVel;
                    projectile.netUpdate = true;
                }
                suppressReturnTicks--;
            }

            // === 新增 A：回程阶段“锁存”判定 ===
            if (projectile.aiStyle == ProjAIStyleID.Boomerang)
            {
                int ai0 = (int)projectile.ai[0];

                // 从外放(0) -> 回程(非0) 且 不在抑制期：进入“真正回程”
                if (lastAi0 == 0 && ai0 != 0 && suppressReturnTicks <= 0)
                {
                    inReturnPhase = true;
                }
                lastAi0 = ai0;
            }
            else
            {
                // 自定义 boomerang：兜底（速度朝向玩家 且 不在抑制期）
                if (Vector2.Dot(projectile.velocity, owner.Center - projectile.Center) > 0f && suppressReturnTicks <= 0)
                    inReturnPhase = true;
            }

            // === 新增 B：归心似箭 + 贴身收敛 + 过线接取 ===
            if (mp.Guixin && inReturnPhase)
            {
                Vector2 toOwner = owner.MountedCenter - projectile.Center;
                float dist = toOwner.Length();
                Vector2 dir = dist > 1f ? toOwner / dist : Vector2.Zero;

                float baseSpeed = initialSpeed > 0.01f ? initialSpeed : projectile.velocity.Length();
                float maxDesired = baseSpeed * 2f;                // 你的“归心似箭”2倍
                float nearLimited = MathF.Min(maxDesired, MathF.Max(6f, dist * 0.5f)); // 距离越近，速度越低

                Vector2 targetVel = dir * nearLimited;
                projectile.velocity = Vector2.Lerp(projectile.velocity, targetVel, 0.5f);

                // —— 接取半径：进入就直接接住，防止拖尾绕身后
                const float catchRadius = 18f; // 约 1.125 格
                if (dist <= catchRadius) { SafeCatch(projectile); return; }

                // —— 过线检测：本帧运动会跨过玩家（避免“飞过头”）
                Vector2 nextPos = projectile.Center + projectile.velocity;
                float dNow = dist;
                float dNext = (owner.MountedCenter - nextPos).Length();
                // 如果 dNext > dNow 且且速度方向不再指向玩家，说明即将/已经越过
                if (Vector2.Dot(projectile.velocity, toOwner) < 0f || dNext > dNow + 2f)
                {
                    // 直接在玩家处接住
                    SafeCatch(projectile);
                    return;
                }
            }

            // 非归心似箭但已在回程阶段，也加一个保底接取，避免极端情况下绕身后
            if (inReturnPhase)
            {
                float dist = Vector2.Distance(owner.MountedCenter, projectile.Center);
                if (dist <= 14f) { SafeCatch(projectile); return; }
            }
        }

        // —— 安全接取：避免残留状态
        private void SafeCatch(Projectile p)
        {
            inReturnPhase = false;
            p.Kill();
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!IsBoomerang(projectile))
                return;

            Player owner = Main.player[projectile.owner];
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
            if (!IsBoomerang(projectile)) return;
            Player owner = Main.player[projectile.owner];
            var mp = owner.GetModPlayer<BoomerangAccessoryPlayer>();

            // ✅ 一旦进入回程阶段（inReturnPhase=true），回程途中每一击都翻倍
            if (mp.Yanfan && inReturnPhase)
                modifiers.SourceDamage *= 2f;
        }

        /// <summary>是否是原版回旋镖AI</summary>
        private static bool IsBoomerang(Projectile p)
        {
            return (p.aiStyle == ProjAIStyleID.Boomerang);
        }
    }
}