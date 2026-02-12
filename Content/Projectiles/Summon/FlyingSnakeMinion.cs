using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles.Summon
{
    public class FlyingSnakeMinion : ModProjectile
    {
        // 使用原版神庙飞蛇的 NPC 贴图来绘制
        public override string Texture => "Terraria/Images/NPC_" + NPCID.FlyingSnake;

        public override bool? CanCutTiles() => false; // 不用于清草
        public override bool MinionContactDamage() => false; // 只靠远程唾液打

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            Main.projPet[Type] = true;
            // 让召唤物能被献祭/替换，且在指挥杖可控制
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 62;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;                 // 占 1 个召唤位（minionSlots 见字段注释） :contentReference[oaicite:3]{index=3}
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 18000;
            Projectile.aiStyle = -1;                      // 自定义 AI
        }

        // 发射间隔
        private const int ShootCooldownMax = 30;
        // 寻敌参数
        private const float TargetAcquireRange = 700f;
        private const float ShootRange = 600f;
        private const float SpitSpeed = 11f;

        // 存活维护 → 计算 idlePos → 仆从离玩家太远，瞬移到玩家附近 -> 寻怪 → 回位/移动 → 朝向/分离 → 动画 → 冷却/开火
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active)
            {
                Projectile.Kill();
                return;
            }

            // 维持召唤物寿命（按你的项目惯例）
            Projectile.timeLeft = 2;

            // —— Idle 位置（拉开 minionPos 的间距） ——
            Vector2 idleBase = owner.Center + new Vector2(-48f * owner.direction, -64f);
            float xSpacing = 28f; // 横向间距
            float ySpacing = 6f;  // 轻微纵向锯齿
            Vector2 idleOffset = new Vector2(-xSpacing * Projectile.minionPos, -(Projectile.minionPos % 3) * ySpacing);
            Vector2 idlePos = idleBase + idleOffset;

            // —— Idle 位置算完后（idlePos 已经有值）加入：超远距离瞬移补位 ——
            // 根据需要改这两个阈值
            const float InstantSnapDist = 2800f;   // 离玩家超过这个距离，直接瞬移
            const float LostBehindDist = 1600f;   // 隔着地形且也很远时，也瞬移

            float distToOwner = Vector2.Distance(Projectile.Center, owner.Center);
            bool lineOfSightToOwner = Collision.CanHitLine(Projectile.Center, 1, 1, owner.Center, 1, 1);

            // 只让 owner 端执行以避免多人不同步
            if (Projectile.owner == Main.myPlayer)
            {
                // 1) 距离极大：无条件瞬移
                if (distToOwner > InstantSnapDist)
                {
                    SnapToOwner(owner, idlePos);
                    return; // 已经到位，本帧后续移动/射击就按新位置算
                }

                // 2) 距离较大且与玩家失去视线：也瞬移
                if (distToOwner > LostBehindDist && !lineOfSightToOwner)
                {
                    SnapToOwner(owner, idlePos);
                    return;
                }
            }

            // —— 寻怪（假定你已有 FindTarget） ——
            NPC target = FindTarget(owner, TargetAcquireRange);
            bool hasTarget = target != null;

            // 无目标时让 idle 位置稍微漂浮一下，更灵动
            if (!hasTarget)
            {
                float t = (float)(Main.timeForVisualEffects / 30.0 + Projectile.minionPos * 0.35);
                idlePos += t.ToRotationVector2() * 10f;
            }

            // —— 回位/跟随（惯性式） ——
            float speed = 10f;
            float inertia = 18f;
            Vector2 toIdle = idlePos - Projectile.Center;

            if (toIdle.Length() > 200f) { speed = 14f; inertia = 12f; }
            if (toIdle.Length() > 20f)
            {
                Vector2 desired = toIdle.SafeNormalize(Vector2.Zero) * speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desired) / inertia;
            }
            else
            {
                Projectile.velocity *= 0.96f;
            }

            // —— 开火前先转身（有目标优先按目标方向） ——
            Vector2? aimDir = hasTarget ? target.Center - Projectile.Center : (Vector2?)null;
            UpdateFacing(owner, hasTarget, aimDir); // ← 不内嵌，调用私有方法
            DoSeparation();                        // ← 速度确定后再做分离

            // —— 帧动画（按你的帧数循环） ——
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }

            // —— 冷却与开火（SalamanderSpit 从“嘴巴”发射） ——
            Projectile.localAI[0] = Math.Max(0f, Projectile.localAI[0] - 1f);

            if (hasTarget && Projectile.owner == Main.myPlayer)
            {
                float dist = Vector2.Distance(Projectile.Center, target.Center);
                if (dist <= ShootRange && Projectile.localAI[0] <= 0f)
                {
                    Vector2 muzzle = GetMuzzleWorldPos(); // ★ 枪口在嘴巴
                    Vector2 shootVel = (target.Center - muzzle).SafeNormalize(Vector2.UnitX) * SpitSpeed;

                    int p = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        muzzle,
                        shootVel,
                        ProjectileID.SalamanderSpit,
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    if (p >= 0 && p < Main.maxProjectiles)
                    {
                        Main.projectile[p].friendly = true;
                        Main.projectile[p].hostile = false;
                        Main.projectile[p].DamageType = DamageClass.Summon;
                        // 可选：Main.projectile[p].usesLocalNPCImmunity = true; Main.projectile[p].localNPCHitCooldown = 10;
                    }

                    Projectile.localAI[0] = ShootCooldownMax;
                }
            }
        }

        // ====== 把下面这些“私有方法”加到同一个类里（AI() 外面） ======

        // 贴图朝向：无敌人且不移动 -> 跟随玩家；否则 -> 跟随“攻击/移动方向”
        // 注意：飞蛇贴图默认“头朝左”，故 spriteDirection = -direction
        private void UpdateFacing(Player owner, bool hasTarget, Vector2? aimDirOrNull)
        {
            bool isMoving = Math.Abs(Projectile.velocity.X) > 0.1f || Math.Abs(Projectile.velocity.Y) > 0.1f;

            if (!hasTarget && !isMoving)
            {
                Projectile.direction = owner.direction;
                Projectile.spriteDirection = -Projectile.direction; // 默认朝左 -> 取反
                return;
            }

            int dir;
            if (hasTarget && aimDirOrNull.HasValue && Math.Abs(aimDirOrNull.Value.X) > 0.01f)
            {
                dir = aimDirOrNull.Value.X >= 0f ? 1 : -1; // 有目标时优先按目标方向
            }
            else
            {
                dir = Projectile.velocity.X >= 0f ? 1 : -1; // 否则按移动方向
            }

            Projectile.direction = dir;
            Projectile.spriteDirection = -dir; // 默认朝左 -> 取反
        }

        // 计算“嘴巴”的世界坐标（以 Center 为基准，自动随左右镜像）
        // 适配 64x62 的 hitbox：这两个偏移量基本能把发射口放在上方靠近嘴的位置
        private Vector2 GetMuzzleWorldPos()
        {
            const float MouthOffsetX = 24f;   // 从中心到嘴的水平距离（可微调 22~28）
            const float MouthOffsetY = -18f;  // 从中心到嘴的垂直距离（向上为负，可微调 -16~-22）

            float x = (Projectile.spriteDirection == 1) ? +MouthOffsetX : -MouthOffsetX;
            float y = MouthOffsetY;

            // 如需更贴合动画，可加 1px 上下轻微抖动：
            // y += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f + Projectile.whoAmI) * 1.0f;

            return Projectile.Center + new Vector2(x, y);
        }

        // 同类同 owner 的仆从之间轻微“排斥”，避免堆叠
        private void DoSeparation()
        {
            const float pushRadius = 36f;    // 认为此距离内就太挤
            const float pushStrength = 0.08f; // 每帧微小推力（0.05~0.12 可调）

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
        // 把仆从瞬移到 idlePos 附近，略带 1~2 像素随机，避免完全重叠
        private void SnapToOwner(Player owner, Vector2 idlePos)
        {
            Vector2 jitter = new Vector2(Main.rand.NextFloat(-12f, 12f), Main.rand.NextFloat(-12f, 12f));
            Projectile.Center = idlePos + jitter;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true; // 同步给服务器/其他客户端
        }

        private NPC FindTarget(Player owner, float acquireRange)
        {
            NPC chosen = null;
            float best = acquireRange;

            // 优先玩家用鞭子标记/指挥的目标
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC forced = Main.npc[owner.MinionAttackTargetNPC];
                if (forced.CanBeChasedBy(this) && Vector2.Distance(Projectile.Center, forced.Center) < best)
                {
                    return forced;
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.CanBeChasedBy(this)) continue;
                float d = Vector2.Distance(Projectile.Center, n.Center);
                if (d < best && Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1))
                {
                    best = d;
                    chosen = n;
                }
            }
            return chosen;
        }
    }
}