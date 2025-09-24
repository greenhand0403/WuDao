using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles.Summon
{
    public class HeroBrokenSwordMinion : ModProjectile
    {
        // 复用断裂英雄剑贴图
        public override string Texture => "Terraria/Images/Item_" + ItemID.BrokenHeroSword;

        // 简单状态机：Idle/接敌巡航、Windup/蓄力、Dash/穿刺、Recover/拉开
        private enum AttackState : int { Idle = 0, Windup = 1, Dash = 2, Recover = 3 }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // 右键点名
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false; // 飞行小兵
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 18000;
            Projectile.scale = 0.8f;
        }

        public override bool MinionContactDamage() => true; // 用接触伤害

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // ——保活：玩家死则清自家 buff；有自家 buff 就把 timeLeft 续成 2——
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<HeroBrokenSwordBuff>());
                return;
            }
            if (player.HasBuff(ModContent.BuffType<HeroBrokenSwordBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            // ——是否是“刃杖小兵”（这里恒为 true，只是沿用原版结构）——
            bool isBladeStaffMinion = true;

            // ——Idle 参考点（玩家头顶略上方），并做“多把分环”分布——
            Vector2 idle = player.Center + new Vector2(0f, -45f) - Vector2.UnitX * 17f;
            if (isBladeStaffMinion && Projectile.ai[0] == 0f)
            {
                // 统计本玩家、同 type 的索引与总数，用于环形分布
                GetMyIndexAndCount(out int index, out int count);
                float step = MathHelper.TwoPi / Math.Max(1, count);
                float period = count * 0.66f; // 原版用来做时间相位滚动
                Vector2 radius = (count > 1 ? new Vector2(30f, 6f) : Vector2.Zero) / 5f * (count - 1);
                Vector2 ringDir = Vector2.UnitY.RotatedBy(step * index + (Main.GlobalTimeWrappedHourly % period) / period * MathHelper.TwoPi);
                idle += ringDir * radius;
                idle.Y += player.gfxOffY;
                idle = idle.Floor(); // 对齐像素
            }

            if (Projectile.ai[0] == 0f)
            {
                // ——Idle 悬停与回位速度上限（距离越远越快，原版写法）——
                Vector2 toIdle = idle - Projectile.Center;
                float maxSpeed = 10f;
                float lerp = Utils.GetLerpValue(200f, 500f, toIdle.Length(), true);
                maxSpeed += lerp * 30f;

                if (toIdle.Length() >= 3000f)
                {
                    Projectile.Center = idle; // 过远直接传送回位（原版）
                }

                // 以 capped 速度直接朝 idle 飞
                Vector2 v = toIdle;
                if (v.Length() > maxSpeed) v *= maxSpeed / v.Length();
                Projectile.velocity = v;

                // ——搜敌（原版：800 距离；命中则进入 Attack 态并记录 NPC 索引到 ai[1]）——
                int targetIndex = -1;
                MinionFindTargetInRange(800, ref targetIndex, skipIfCannotHitWithOwnBody: false);
                if (targetIndex != -1)
                {
                    Projectile.ai[0] = 60f; // 原版就是随便给个正值进入“攻击态”
                    Projectile.ai[1] = targetIndex;
                    Projectile.netUpdate = true;
                }
                else
                {
                    // ——空闲时的朝向：指向速度方向；若已就位则保持朝上——
                    Projectile.rotation = -MathHelper.PiOver4;
                }

                // ——与同伴分离（避免重叠）——
                SeparateFromSameType(0.1f, Projectile.width * 5);
                return;
            }

            if (Projectile.ai[0] == -1f)
            {
                // ——Recover 段：短暂刹车旋转几帧（Dust/音效可加可不加）——
                Projectile.ai[1] += 1f;
                Projectile.velocity *= 0.92f;
                if (Projectile.ai[1] >= 9f)
                {
                    Projectile.ai[0] = 0f;
                    Projectile.ai[1] = 0f;
                }
                return;
            }

            // ——Attack 段：追击已记录的 NPC；失效则转入 Recover 或 Idle——
            NPC target = null;
            int npcIndex = (int)Projectile.ai[1];
            if (npcIndex >= 0 && npcIndex < Main.maxNPCs && Main.npc[npcIndex].CanBeChasedBy(this))
            {
                target = Main.npc[npcIndex];
            }

            if (target == null)
            {
                Projectile.ai[0] = -1f; // 原版：目标无效时转 Recover
                Projectile.ai[1] = 0f;
                Projectile.netUpdate = true;
            }
            else if (player.Distance(target.Center) >= 900f)
            {
                Projectile.ai[0] = 0f;  // 离玩家太远，回 Idle
                Projectile.ai[1] = 0f;
                Projectile.netUpdate = true;
            }
            else
            {
                // ——直线追击，速度上限 16（原版“贴身输出”的关键）——
                Vector2 toTarget = target.Center - Projectile.Center;
                float chaseMax = 16f;
                Vector2 v = toTarget;
                if (v.Length() > chaseMax) v *= chaseMax / v.Length();
                // Projectile.velocity = v;
                Vector2 dash = toTarget.SafeNormalize(Vector2.UnitX) * 18f;
                Projectile.velocity = dash;
                Projectile.ai[0] = -1f;   // 立刻进入减速回收几帧
                Projectile.ai[1] = 0f;
                Projectile.netUpdate = true;
                // 攻击时剑尖跟随速度方向并偏转角度，恰好指向敌人
                Projectile.rotation = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation() + MathHelper.PiOver4;
            }

            // ——同伴分离——
            SeparateFromSameType(0.1f, Projectile.width * 5);
        }
        private void GetMyIndexAndCount(out int index, out int count)
        {
            index = 0; count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner && p.type == Projectile.type)
                {
                    if (i < Projectile.whoAmI) index++;
                    count++;
                }
            }
        }

        private void MinionFindTargetInRange(int range, ref int attackTarget, bool skipIfCannotHitWithOwnBody)
        {
            attackTarget = -1;
            float dist = range;
            // 右键点名优先
            Player owner = Main.player[Projectile.owner];
            if (owner.HasMinionAttackTargetNPC)
            {
                int i = owner.MinionAttackTargetNPC;
                if (Main.npc[i].CanBeChasedBy(this))
                {
                    float d = Vector2.Distance(Projectile.Center, Main.npc[i].Center);
                    if (d < dist) { attackTarget = i; dist = d; }
                }
            }
            // 普通搜敌
            if (attackTarget == -1)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (!n.CanBeChasedBy(this)) continue;
                    float d = Vector2.Distance(Projectile.Center, n.Center);
                    if (d < dist)
                    {
                        if (skipIfCannotHitWithOwnBody && !Collision.CanHit(Projectile.Center, 1, 1, n.Center, 1, 1))
                            continue;
                        attackTarget = i; dist = d;
                    }
                }
            }
        }

        private void SeparateFromSameType(float push, float manhattanRange)
        {
            for (int j = 0; j < Main.maxProjectiles; j++)
            {
                if (j == Projectile.whoAmI) continue;
                Projectile q = Main.projectile[j];
                if (q.active && q.owner == Projectile.owner && q.type == Projectile.type &&
                    Math.Abs(Projectile.position.X - q.position.X) + Math.Abs(Projectile.position.Y - q.position.Y) < manhattanRange)
                {
                    Projectile.velocity.X += (Projectile.position.X < q.position.X) ? -push : push;
                    Projectile.velocity.Y += (Projectile.position.Y < q.position.Y) ? -push : push;
                }
            }
        }
    }
}
