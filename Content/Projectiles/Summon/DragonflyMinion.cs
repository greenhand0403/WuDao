using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles.Summon
{
    // 蝴蝶仆从
    public class DragonflyMinion : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.BlackDragonfly;
        public override bool MinionContactDamage() => true;
        public override bool? CanCutTiles() => false;
        private ref float Variant => ref Projectile.ai[0];      // 已同步
        private ref float State => ref Projectile.ai[1];        // 改这里
        private ref float StateTimer => ref Projectile.localAI[1];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 12;

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f; // 仍然占1槽，但我们额外强制最多7只

            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;

            Projectile.tileCollide = false; // 飞行随从，避免卡地形
            // Projectile.ignoreWater = true;

            Projectile.DamageType = DamageClass.Summon;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
        }
        private const int State_Idle = 0;
        private const int State_Observe = 1;
        private const int State_Strike = 2;
        private int dragonflyID;
        public override void OnSpawn(IEntitySource source)
        {
            // 加载贴图
            // 0..6
            int variant = ((int)Projectile.ai[0] % 7 + 7) % 7;

            dragonflyID = NPCID.BlackDragonfly + variant; // 595..601

            Main.instance.LoadNPC(dragonflyID);
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            // 1) GeneralBehavior / MaintainBuff
            if (!owner.active || owner.dead || !owner.HasBuff(ModContent.BuffType<DragonflyCaneBuff>()))
            {
                owner.ClearBuff(ModContent.BuffType<DragonflyCaneBuff>());
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<DragonflyCaneBuff>()))
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
            Vector2 idlePos = Vector2.Zero;
            // 3) Movement (State Machine)
            if (!hasTarget)
            {
                // 没目标：回到 Idle
                State = State_Idle;
                StateTimer = 0f;

                GetIdleSlot(owner, out int index, out int total);
                // 7只蜻蜓圆环分布
                // float radius = 65f;
                // float height = -85f;
                // float angle = MathHelper.TwoPi * (index / (float)total);

                // idlePos = owner.Center
                //     + new Vector2(radius, 0f).RotatedBy(angle)
                //     + new Vector2(0f, height);
                // 7只横排：让 index=0..6 映射成 -3..+3
                int mid = (total - 1) / 2;          // total=7 => mid=3
                float spacing = 26f;               // 每只间距（按你贴图大小调，24~32都行）
                float yOffset = -90f;              // 头顶高度（按你想要的高度调）

                idlePos = owner.Center + new Vector2((index - mid) * spacing, yOffset);

                HoverTo(idlePos, speed: 10f, inertia: 18f);

                // 4) Visuals：悬停严格跟玩家方向，不要用 idlePos 决定方向
                UpdateVisuals(owner, hasTarget: false, idlePos);
                return;
            }

            // 有目标：蜻蜓点水式攻击（观望→蛰一下→观望循环）
            DoDragonflyAttack(owner, target);

            // 视觉：你现在的 UpdateVisuals 已经很好用（悬停贴玩家、移动贴速度）
            UpdateVisuals(owner, hasTarget: true, idlePos);
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
        private void DoDragonflyAttack(Player owner, NPC target)
        {
            // 观望点：目标上方+左右摆动（用 whoAmI 做相位差，避免一堆蜻蜓重叠）
            Vector2 observePos = target.Center + new Vector2(0f, -80f);
            float sway = (float)Math.Sin((Main.GameUpdateCount + Projectile.whoAmI * 13) * 0.08f) * 80f;
            observePos.X += sway;

            // “点水蛰一下”的落点：敌人头顶
            Vector2 strikePos = target.Top + new Vector2(0f, -8f);

            switch (State)
            {
                case State_Idle:
                    State = State_Observe;
                    StateTimer = 0f;
                    break;

                case State_Observe:
                    StateTimer++;

                    // 观望：平滑飞到 observePos
                    HoverTo(observePos, speed: 12f, inertia: 14f);

                    // 观望一会儿 → 进行一次点水
                    if (StateTimer >= 50f)
                    {
                        State = State_Strike;
                        StateTimer = 0f;
                        Projectile.netUpdate = true;
                    }
                    break;

                case State_Strike:
                    StateTimer++;

                    // 点水：快速冲到敌人头顶（不像蝴蝶那样穿来穿去）
                    Vector2 to = strikePos - Projectile.Center;
                    Vector2 desiredVel = to.SafeNormalize(Vector2.UnitY) * 18f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.45f);

                    // 到达头顶附近 或 超时 → 回观望
                    if (to.Length() < 14f || StateTimer >= 14f)
                    {
                        State = State_Observe;
                        StateTimer = 0f;
                        Projectile.netUpdate = true;
                    }
                    break;
            }
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

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Npc[dragonflyID].Value;
            int frames = Main.npcFrameCount[dragonflyID];

            if (frames <= 0) frames = 1;

            int frame = (int)((Main.GameUpdateCount / 4) % frames);

            int frameHeight = tex.Height / frames;
            Rectangle src = new Rectangle(0, frame * frameHeight, tex.Width, frameHeight);

            SpriteEffects fx = (Projectile.spriteDirection == 1)
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            Vector2 origin = src.Size() * 0.5f;

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                src,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                fx,
                0
            );

            return false;
        }
    }
}