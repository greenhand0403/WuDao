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
    // 太极剑仆从：12帧动画（前6帧样式1穿甲，后6帧样式2吸血），82x82；AI整合蝴蝶仆从与断裂英雄剑思路，并模仿附魔剑敌怪的突进节奏
    public class TaijiSwordMinion : ModProjectile
    {
        public override bool MinionContactDamage() => true;
        public override bool? CanCutTiles() => true;

        // ai[0] = style (0/1)
        // ai[1] = initFlag
        // localAI[0] = state (0=盘旋, 1=接敌巡航, 2=俯冲)
        // localAI[1] = timer (状态计时)
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6; // 12帧：前6帧样式1（穿甲），后6帧样式2（吸血）
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 82;
            Projectile.height = 82;

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f; // 仍然占1槽，但我们额外强制最多2只（与蝴蝶仆从类似）

            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;

            Projectile.tileCollide = false; // 飞行随从，避免卡地形

            Projectile.DamageType = DamageClass.Summon;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
        }

        // 建议：把状态扩展成 3 个（更清晰）
        private const int State_Idle = 0;
        private const int State_Staging = 1;
        private const int State_Dash = 2;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // 1) GeneralBehavior / MaintainBuff
            // 如果“太极剑匣”用的是别的 Buff，请把这里替换成你的新 BuffType。
            if (!owner.active || owner.dead || !owner.HasBuff(ModContent.BuffType<TaijiSwordBoxBuff>()))
            {
                owner.ClearBuff(ModContent.BuffType<TaijiSwordBoxBuff>());
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<TaijiSwordBoxBuff>()))
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
            Vector2 idlePos = Vector2.Zero;

            GetIdleSlot(owner, out int index, out int total);

            // 3) Movement (State Machine)
            if (!hasTarget)
            {
                // 没目标：回到 Idle
                state = State_Idle;
                Projectile.localAI[0] = state;
                Projectile.localAI[1] = 0f;

                float radius = 40f;
                float height = -70f;
                float angle = MathHelper.TwoPi * (index / (float)total);

                idlePos = owner.Center
                    + new Vector2(radius, 0f).RotatedBy(angle)
                    + new Vector2(0f, height);

                HoverTo(idlePos, speed: 18f, inertia: 8f);
                // ✅到位后直接把速度归零，消除抖动
                if (Vector2.DistanceSquared(Projectile.Center, idlePos) < 4f) // 2px 半径
                {
                    Projectile.Center = idlePos;
                    Projectile.velocity = Vector2.Zero;
                }
                // ——空闲时的朝向：指向速度方向；若已就位则保持朝上——
                Projectile.rotation = -MathHelper.PiOver4;
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
                float side = (index == 0) ? -60f : 60f; // ✅两把剑左右分开
                // 加一点随机偏移
                Vector2 stagingPos = target.Center + new Vector2(side, 70f * Main.rand.NextFloat(-2f, 2f));
                // 正常速度 18f，惯性越大转向越慢 8f
                HoverTo(stagingPos, speed: 18f, inertia: 8f);

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

                // 第1帧给一个冲刺初速度（之后主要靠惯性飞）
                if (Projectile.localAI[1] == 1f)
                {
                    Projectile.velocity = to.SafeNormalize(Vector2.UnitY) * 24f; // 可调：穿刺速度
                }
                else
                {
                    // ✅减少“追踪修正”，避免蛇形贴转（建议直接去掉 Lerp）
                    Projectile.velocity *= 0.98f; // 轻微阻尼
                    // 阻尼越小，减速越快，则惯性滑行距离越短
                }

                // ✅关键：Dash 持续固定时间后回到巡航位，形成“不断穿刺”
                // 16 控制惯性距离
                if (Projectile.localAI[1] >= 16f)
                {
                    Projectile.localAI[0] = State_Staging;
                    Projectile.localAI[1] = 0f;
                    Projectile.netUpdate = true;
                }
            }

            // 攻击时剑尖跟随速度方向并偏转角度，恰好指向敌人
            Projectile.rotation = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation() + MathHelper.PiOver4;
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

            // 扫描所有投射物，找到属于该玩家的 TaijiSwordMinion
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

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 样式1：护甲穿透（等价于“附魔剑”那种更容易打出伤害的感觉）
            if ((int)Projectile.ai[0] == 1)
            {
                modifiers.ArmorPenetration += 15; // 你可以按需要调高/调低
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 样式2：击中回复 2 点生命
            if ((int)Projectile.ai[0] == 0)
            {
                Player owner = Main.player[Projectile.owner];
                if (owner != null && owner.active && !owner.dead)
                {
                    // 单人/服务器端执行：避免纯客户端改血导致不同步
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        owner.Heal(2);
                    }
                    else if (Main.myPlayer == owner.whoAmI)
                    {
                        // 多人客户端兜底：至少本地先显示回血效果（严格同步可改为自定义 ModPacket 让服务器加血）
                        owner.Heal(2);
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            int style = ((int)Projectile.ai[0] % 2 + 2) % 2; // 0=样式1（前6帧），1=样式2（后6帧）

            // 帧：0..5（每4 tick换一次）
            int frame = (int)((Main.GameUpdateCount / 4) % 6);
            int srcFrame = style * 6 + frame;
            int srcY = srcFrame * Projectile.height;

            Rectangle src = new Rectangle(0, srcY, Projectile.width, Projectile.height);

            Vector2 origin = src.Size() * 0.5f;

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                src,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}