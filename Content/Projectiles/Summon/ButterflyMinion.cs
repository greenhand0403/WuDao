using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles.Summon
{
    // 蝴蝶仆从
    public class ButterflyMinion : ModProjectile
    {
        // 复用“蝴蝶合集图” 24x576（NPC_356）
        public override string Texture => "Terraria/Images/NPC_" + NPCID.Butterfly;

        // ai[0] = variantIndex (0..7)
        // ai[1] = 0 (未使用)
        // localAI[0] = state (0=盘旋, 1=俯冲)
        // localAI[1] = timer (状态计时)
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3; // 三帧动画
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24; // 你说的单帧24x24 把4帧间隔也算进去了

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f; // 仍然占1槽，但我们额外强制最多8只

            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;

            Projectile.tileCollide = false; // 飞行随从，避免卡地形
            // Projectile.ignoreWater = true;

            Projectile.DamageType = DamageClass.Summon;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
        }

        public override bool MinionContactDamage() => true;
        public override bool? CanCutTiles() => false;
        // 建议：把状态扩展成 3 个（更清晰）
        private const int State_Idle = 0;
        private const int State_Staging = 1;
        private const int State_Dash = 2;
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // 1) GeneralBehavior / MaintainBuff
            if (!owner.active || owner.dead)
            {
                owner.ClearBuff(ModContent.BuffType<ButterflyCaneBuff>());
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<ButterflyCaneBuff>()))
                Projectile.timeLeft = 2;

            // 防迷路瞬移
            if (Vector2.Distance(Projectile.Center, owner.Center) > 2000f)
            {
                Projectile.Center = owner.Center;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            }

            // 2) SearchTarget
            int targetIndex = FindTarget(owner, 800f);
            bool hasTarget = targetIndex != -1;
            NPC target = hasTarget ? Main.npc[targetIndex] : null;

            // 状态与计时器：沿用你的 localAI
            int state = (int)Projectile.localAI[0];
            Projectile.localAI[1]++; // stateTimer
            Vector2 idlePos=Vector2.Zero;
            // 3) Movement (State Machine)
            if (!hasTarget)
            {
                // 没目标：回到 Idle
                state = State_Idle;
                Projectile.localAI[0] = state;
                Projectile.localAI[1] = 0f;

                GetIdleSlot(owner, out int index, out int total);

                float radius = 40f;
                float height = -70f;
                float angle = MathHelper.TwoPi * (index / (float)total);

                idlePos = owner.Center
                    + new Vector2(radius, 0f).RotatedBy(angle)
                    + new Vector2(0f, height);

                HoverTo(idlePos, speed: 10f, inertia: 18f);

                // 4) Visuals：悬停严格跟玩家方向，不要用 idlePos 决定方向
                UpdateVisuals(owner, hasTarget: false,idlePos);
                return;
            }

            // 有目标：攻击逻辑
            if (state == State_Idle)
            {
                // 刚发现目标：进入 Staging
                state = State_Staging;
                Projectile.localAI[0] = state;
                Projectile.localAI[1] = 0f;
                Projectile.netUpdate = true;
            }

            if (state == State_Staging)
            {
                Vector2 stagingPos = target.Center + new Vector2(0f, -70f);
                HoverTo(stagingPos, speed: 11f, inertia: 14f);

                if (Vector2.Distance(Projectile.Center, stagingPos) < 26f)
                {
                    state = State_Dash;
                    Projectile.localAI[0] = state;
                    Projectile.localAI[1] = 0f;
                    Projectile.netUpdate = true;
                }
            }
            else // State_Dash
            {
                Vector2 to = target.Center - Projectile.Center;

                // Dash 速度/转向
                Projectile.velocity = Vector2.Lerp(
                    Projectile.velocity,
                    to.SafeNormalize(Vector2.UnitY) * 16f,
                    0.35f
                );

                // Dash 持续一小段时间后回到 Staging 或 Idle
                if (Projectile.localAI[1] > 22f || to.Length() < 16f)
                {
                    state = State_Staging; // 你也可以改回 Idle：看你想不想连段
                    Projectile.localAI[0] = state;
                    Projectile.localAI[1] = 0f;
                    Projectile.netUpdate = true;
                }
            }

            // 4) Visuals：攻击时方向跟速度，旋转让头对齐速度方向
            UpdateVisuals(owner, hasTarget: true,idlePos);
        }

        private void UpdateVisuals(Player owner, bool hasTarget,Vector2 idlePos)
        {
            if (!hasTarget && Vector2.DistanceSquared(Projectile.Center, idlePos) < 400f)
            {
                // 悬停：严格跟玩家方向，不允许任何“目标点 dx”触发翻面
                Projectile.direction = owner.direction;
                Projectile.spriteDirection = owner.direction;

                // 悬停不旋转（保持默认“左上45°”观感）
                Projectile.rotation = 0f;

                // 这里不需要翻面冷却，因为方向完全由玩家决定，不会抖
                return;
            }

            // 攻击：方向跟速度（避免 vx≈0 时抖动）
            float vx = Projectile.velocity.X;
            if (Math.Abs(vx) > 0.2f)
            {
                Projectile.direction = (vx > 0f) ? 1 : -1;
                Projectile.spriteDirection = Projectile.direction;
            }
            else
            {
                // 速度太小：保持上一帧方向（别在 0 附近左右跳）
                if (Projectile.spriteDirection == 0)
                {
                    Projectile.direction = owner.direction;
                    Projectile.spriteDirection = owner.direction;
                }
            }

            UpdateRotationToVelocity();
        }

        private void UpdateRotationToVelocity()
        {
            if (Projectile.velocity.LengthSquared() < 0.05f)
            {
                Projectile.rotation = 0f;
                return;
            }

            float velAngle = Projectile.velocity.ToRotation();

            // 贴图默认头：左上45° => (-1,-1)；翻转后头：右上45° => (1,-1)
            float headAngle = (Projectile.spriteDirection == 1)
                ? new Vector2(1f, -1f).ToRotation()   // -45°
                : new Vector2(-1f, -1f).ToRotation(); // -135°

            Projectile.rotation = velAngle - headAngle;
        }

        private void HoverTo(Vector2 targetPos, float speed, float inertia)
        {
            Vector2 to = targetPos - Projectile.Center;
            Vector2 desired = to.SafeNormalize(Vector2.Zero) * speed;
            Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desired) / inertia;

            if (to.Length() < 10f)
                Projectile.velocity *= 0.85f;
        }
        private void GetIdleSlot(Player owner, out int index, out int total)
        {
            index = 0;
            total = 0;

            // 扫描所有投射物，找到属于该玩家的 ButterflyMinion
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active) continue;
                if (p.owner != owner.whoAmI) continue;
                if (p.type != Type) continue;

                total++;
            }

            if (total <= 1)
            {
                index = 0;
                total = 1;
                return;
            }

            // 第二遍：按 whoAmI 排序意义上的 index（小的排前面）
            int rank = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active) continue;
                if (p.owner != owner.whoAmI) continue;
                if (p.type != Type) continue;

                if (p.whoAmI == Projectile.whoAmI)
                {
                    index = rank;
                    return;
                }

                // 统计有多少 whoAmI 比我小的同类
                if (p.whoAmI < Projectile.whoAmI)
                    rank++;
            }

            // 兜底
            index = 0;
        }

        private int FindTarget(Player owner, float maxRange)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                int idx = owner.MinionAttackTargetNPC;
                NPC npc = Main.npc[idx];
                if (npc.CanBeChasedBy(this) && Vector2.Distance(owner.Center, npc.Center) <= maxRange)
                    return idx;
            }

            int best = -1;
            float bestDist = maxRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this)) continue;

                float d = Vector2.Distance(Projectile.Center, npc.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>
        /// ✅ 手动绘制：按你给的蝴蝶合集贴图规则切 sourceRect
        /// 贴图：24x576
        /// 8种蝴蝶，每种块高72
        /// 每种内：3帧，每帧高20，底部空4
        /// 帧间隔：4 tick
        /// </summary>
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            int variant = ((int)Projectile.ai[0] % 8 + 8) % 8; // 0..7
            int variantBaseY = variant * 72;

            // 帧：0,1,2（每4 tick换一次）
            int frame = (int)((Main.GameUpdateCount / 4) % 3);
            int srcY = variantBaseY + frame * 24;// 每帧包含帧间隔总长度24

            Rectangle src = new Rectangle(0, srcY, 24, 24);

            SpriteEffects fx = (Projectile.spriteDirection == 1)
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            Vector2 origin = src.Size() * 0.5f;

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                src,
                lightColor,
                Projectile.rotation,   // ✅ 这里用 rotation
                origin,
                Projectile.scale,
                fx,
                0
            );

            return false;
        }
    }
}