using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles.Summon
{
    public class ThousandGhostMinion : ModProjectile
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.Ghost}";
        // 寻敌参数
        private const float TargetAcquireRange = 700f;
        public override void SetStaticDefaults()
        {
            // Sets the amount of frames this minion has on its spritesheet
            Main.projFrames[Projectile.type] = 4;
            // This is necessary for right-click targeting
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

            Main.projPet[Projectile.type] = true; // Denotes that this projectile is a pet or minion

            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
        }
        public sealed override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 56;
            Projectile.tileCollide = false; // Makes the minion go through tiles freely

            // These below are needed for a minion weapon
            Projectile.friendly = true; // Only controls if it deals damage to enemies on contact (more on that later)
            Projectile.minion = true; // Declares this as a minion (has many effects)
            Projectile.DamageType = DamageClass.Summon; // Declares the damage type (needed for it to deal damage)
            Projectile.minionSlots = 1f; // Amount of slots this minion occupies from the total minion slots available to the player (more on that later)
            Projectile.penetrate = -1; // Needed so the minion doesn't despawn on collision with enemies or tiles

            // ★ 本地免疫帧：同个 NPC 的受击冷却
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10; // 约 1/6 秒，可按手感 8~20 调整
        }

        // Here you can decide if your minion breaks things like grass or pots
        public override bool? CanCutTiles()
        {
            return false;
        }

        // This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
        public override bool MinionContactDamage()
        {
            return true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // 维持寿命（典型 minion 保活）
            Projectile.timeLeft = 2;

            // —— Idle 点（在玩家背后上方），并用 minionPos 拉开队列 ——
            // Vector2 idleBase = owner.Center + new Vector2(-48f * owner.direction, -64f);
            // float xSpacing = 28f;
            // float ySpacing = 6f;
            // Vector2 idleOffset = new Vector2(-xSpacing * Projectile.minionPos, -(Projectile.minionPos % 3) * ySpacing);
            // Vector2 idlePos = idleBase + idleOffset;

            // —— Idle 位置：玩家上方 ±45° 的扇形内，半径随机 8~16 格（128~256px） ——
            EnsureIdleRadius(); // 首次赋随机半径（像素）

            // 统计该玩家名下同类仆从的“顺位”和总数，用于均匀分配角度
            (int order, int total) = GetOwnedOrderAndCount();

            // 角度（度）：-145° ~ +145° 之间按顺位均匀分布
            float baseDeg = MathHelper.Lerp(-145f, 145f, (order + 1f) / (total + 1f));

            // 轻微摆动（±6°），让待机更有生命力
            float wobbleDeg = 6f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f + Projectile.whoAmI);

            // 最终角度（弧度），相对“正上方(0,-1)”旋转
            float angleRad = MathHelper.ToRadians(baseDeg + wobbleDeg);

            // 方向从“正上方”旋转得到（注意 Y 轴朝下，因此上方是 (0,-1)）
            Vector2 arcDir = new Vector2(0f, -1f).RotatedBy(angleRad);

            // Idle 目标点
            Vector2 idlePos = owner.Center + arcDir * Projectile.localAI[1];


            // —— 寻怪（保持你原来的 FindTarget，如果名字不同就替换） ——
            NPC target = FindTarget(owner, TargetAcquireRange);
            bool hasTarget = target != null;

            // 无目标：让 idle 位置小幅环绕，减少僵直
            if (!hasTarget)
            {
                float t = (float)(Main.timeForVisualEffects / 30.0 + Projectile.minionPos * 0.35);
                idlePos += t.ToRotationVector2() * 10f;
            }

            // —— 远距/隔墙：瞬移补位（玩家用晶塔/海螺后能立刻跟上） ——
            TrySnapToOwner(owner, idlePos);

            // —— 速度调优：提高巡航/追击速度，并按距离动态调惯性 ——
            // 速度基线（比示例更快）
            float cruiseSpeed = hasTarget ? 14f : 11f;  // 追击更快
            float inertia = hasTarget ? 16f : 18f;  // 追击时更灵活

            Vector2 desiredVel;

            if (hasTarget)
            {
                // 直扑目标；可以做一个“提前量”微调：用目标速度的少量分量，减少绕圈
                Vector2 toTarget = target.Center - Projectile.Center;
                Vector2 predict = target.velocity * 6f; // 适度提前量
                Vector2 aim = (toTarget + predict);

                desiredVel = aim.SafeNormalize(Vector2.Zero) * cruiseSpeed;

                // 距离越远，越急（再降一点惯性、提速）
                float d = toTarget.Length();
                if (d > 480f) { inertia -= 3f; cruiseSpeed += 2f; }
                if (d > 800f) { inertia -= 2f; cruiseSpeed += 2f; }

                // 贴靠助推：当离目标还有一段距离时，适当再推一点，避免“追不紧”
                if (d > 140f)
                {
                    desiredVel += toTarget.SafeNormalize(Vector2.Zero) * 0.8f;
                }
            }
            else
            {
                // 无目标：回到 Idle
                Vector2 toIdle = idlePos - Projectile.Center;
                if (toIdle.Length() > 20f)
                {
                    desiredVel = toIdle.SafeNormalize(Vector2.Zero) * cruiseSpeed;
                }
                else
                {
                    desiredVel = Projectile.velocity * 0.96f; // 漂浮
                }
            }

            // —— 惯性移动融合 ——
            Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desiredVel) / inertia;

            // —— 朝向：攻击期朝向目标，否则按规则（无敌人且不移动 -> 跟玩家；其余 -> 跟移动/目标方向） ——
            Vector2? aimDir = hasTarget ? (Vector2?)(target.Center - Projectile.Center) : null;
            UpdateFacing(owner, hasTarget, aimDir);

            // —— 分离：避免多只堆叠（如希望集火时更紧凑，可只在 !hasTarget 时调用） ——
            if (!hasTarget)
            {
                DoSeparation();
            }

            // —— 帧动画：简单 4 帧循环（与 Ghost NPC 观感接近） ——
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }
        }
        // 首次给 localAI[1] 赋“随机半径”（单位：像素）
        // 6~8 格，每格 16 像素 => 128~256 px
        private void EnsureIdleRadius()
        {
            if (Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[1] = Main.rand.Next(6, 8) * 16f;
            }
        }

        // 统计同 owner + 同 type 的该仆从的顺位（按 whoAmI）与总数
        private (int index, int count) GetOwnedOrderAndCount()
        {
            int count = 0;
            int index = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != Projectile.owner || p.type != Projectile.type)
                    continue;

                if (i < Projectile.whoAmI)
                    index++;   // whoAmI 更小的算在前面 -> 稳定顺位

                count++;
            }
            return (index, count);
        }

        // 朝向更新：无敌人且不移动 -> 跟随玩家；否则 -> 跟“目标/移动方向”
        // 注意：Ghost 贴图默认“头朝左”，所以用 spriteDirection = -direction
        private void UpdateFacing(Player owner, bool hasTarget, Vector2? aimDirOrNull)
        {
            bool isMoving = Math.Abs(Projectile.velocity.X) > 0.1f || Math.Abs(Projectile.velocity.Y) > 0.1f;

            if (!hasTarget && !isMoving)
            {
                Projectile.direction = owner.direction;
                Projectile.spriteDirection = -Projectile.direction;
                return;
            }

            int dir;
            if (hasTarget && aimDirOrNull.HasValue && Math.Abs(aimDirOrNull.Value.X) > 0.01f)
            {
                dir = aimDirOrNull.Value.X >= 0f ? 1 : -1;  // 优先看向目标所在方向
            }
            else
            {
                dir = Projectile.velocity.X >= 0f ? 1 : -1; // 退化为移动方向
            }

            Projectile.direction = dir;
            Projectile.spriteDirection = -dir; // 贴图默认朝左 -> 取反
        }

        // 远距/隔墙瞬移补位：解决“玩家传送后跟不上”的问题
        private void TrySnapToOwner(Player owner, Vector2 idlePos)
        {
            const float InstantSnapDist = 2800f; // 极远：必瞬移
            const float LostBehindDist = 1600f; // 远且无视线：也瞬移

            float distToOwner = Vector2.Distance(Projectile.Center, owner.Center);
            bool lineOfSight = Collision.CanHitLine(Projectile.Center, 1, 1, owner.Center, 1, 1);

            if (Projectile.owner != Main.myPlayer) return;

            if (distToOwner > InstantSnapDist || (!lineOfSight && distToOwner > LostBehindDist))
            {
                Vector2 jitter = new Vector2(Main.rand.NextFloat(-12f, 12f), Main.rand.NextFloat(-12f, 12f));
                Projectile.Center = idlePos + jitter;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            }
        }

        // 同类同 owner 的仆从间做轻微排斥，减少堆叠
        private void DoSeparation()
        {
            const float pushRadius = 36f;
            const float pushStrength = 0.08f;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || i == Projectile.whoAmI) continue;
                if (other.owner != Projectile.owner || other.type != Projectile.type) continue;

                float dist = Vector2.Distance(Projectile.Center, other.Center);
                if (dist > 0f && dist < pushRadius)
                {
                    Vector2 push = (Projectile.Center - other.Center).SafeNormalize(Vector2.Zero) * pushStrength;
                    Projectile.velocity += push;
                }
            }
        }
        private NPC FindTarget(Player owner, float acquireRange)
        {
            // 1) 优先玩家鞭子标记
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC forced = Main.npc[owner.MinionAttackTargetNPC];
                if (forced.CanBeChasedBy(this) && Vector2.Distance(Projectile.Center, forced.Center) <= acquireRange)
                    return forced;
            }

            // 2) 一般索敌：不强制视线（近战/穿墙类仆从）
            NPC best = null;
            float bestDist = acquireRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.CanBeChasedBy(this))
                    continue;

                float d = Vector2.Distance(Projectile.Center, n.Center);
                if (d <= bestDist)
                {
                    // 若需要，可给“有视线”一点加权，但不做硬性要求
                    // bool hasLos = Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1);
                    // float score = d + (hasLos ? 0f : 120f); // 例：没视线就劣化分数
                    best = n;
                    bestDist = d;
                }
            }

            return best;
        }
    }
}