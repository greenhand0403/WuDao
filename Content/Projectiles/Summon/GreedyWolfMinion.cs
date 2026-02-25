using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using WuDao.Content.Buffs;
using Terraria.Utilities;
using System;

namespace WuDao.Content.Projectiles.Summon
{
    public class GreedyWolfMinion : ModProjectile
    {
        // 复用原版敌怪狼贴图（NPCID==155，13帧，单行横向切帧）
        public override string Texture => "Terraria/Images/NPC_" + NPCID.Wolf;

        public override bool MinionContactDamage() => true;
        public override bool? CanCutTiles() => false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 13;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 48-4;

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;

            // ✅ 不穿墙：始终撞方块
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.DamageType = DamageClass.Summon;

            // 接触伤害 + 本地无敌帧
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead || !owner.HasBuff(ModContent.BuffType<GreedyWolfBuff>()))
            {
                owner.ClearBuff(ModContent.BuffType<GreedyWolfBuff>());
                Projectile.Kill();
                return;
            }

            // 维持buff
            Projectile.timeLeft = 2;

            // 初始化一次随机偏移（每只狼不同）
            if (Projectile.localAI[2] == 0f)
            {
                Projectile.localAI[2] = 1f;
                // 用 whoAmI 做种子，保证每只不一样且稳定
                UnifiedRandom r = new UnifiedRandom(Projectile.whoAmI * 1337 + Projectile.owner * 17);
                Projectile.ai[1] = r.NextFloat(-80f, 80f); // 横向随机范围（像素）
            }
            float randX = Projectile.ai[1];

            Vector2 idlePos = owner.Bottom
    + new Vector2(owner.direction * -60f + randX, -Projectile.height / 2f);

            int target = FindTarget(owner, 700f);
            float distToOwner = Vector2.Distance(Projectile.Center, owner.Center);

            // 跑丢兜底：太远直接拉回（避免永远卡墙外）
            if (distToOwner > 2000f)
            {
                Projectile.Center = owner.Center;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
                return;
            }

            bool onGround = IsOnGround();

            // 目标点：有怪追怪，没怪回玩家
            Vector2 dest = (target != -1) ? Main.npc[target].Center : idlePos;
            Vector2 toDest = dest - Projectile.Center;

            // ========== 方向 ==========
            if (toDest.X > 12f)
                Projectile.direction = 1;
            else if (toDest.X < -12f)
                Projectile.direction = -1;

            // ========== 重力 ==========
            if (!onGround)
            {
                Projectile.velocity.Y += 0.45f;
                if (Projectile.velocity.Y > 12f)
                    Projectile.velocity.Y = 12f;
            }
            else
            {
                if (Projectile.velocity.Y > 0f)
                    Projectile.velocity.Y = 0.2f;
            }

            // ========== 地面奔跑（模仿“狼的贴地冲刺”感觉） ==========
            float maxSpeed = (target != -1) ? 8.0f : 6.5f;
            float accel = (target != -1) ? 0.45f : 0.35f;

            // 没目标且离玩家很近：慢慢停下来（更像待机）
            float distToDest = toDest.Length();
            if (target == -1 && distToDest < 70f)
            {
                Projectile.velocity.X *= 0.85f;
            }
            else
            {
                float desiredX = Projectile.direction * maxSpeed;
                Projectile.velocity.X = Approach(Projectile.velocity.X, desiredX, accel);
            }

            // ========== 跳跃：遇到障碍/台阶 或 目标在高处 ==========
            if (onGround)
            {
                bool obstacleAhead = HasObstacleAhead(Projectile.direction);
                bool targetHigher = (target != -1) && (Main.npc[target].Center.Y < Projectile.Center.Y - 40f);

                if ((obstacleAhead || targetHigher) && Projectile.velocity.Y >= 0f)
                {
                    // 跳一下，不搞复杂：就像原版狼/地面怪那种简易跨越
                    Projectile.velocity.Y = -7.5f;
                    Projectile.netUpdate = true;
                }
            }
            // 简易飞扑触发：靠近目标且在地面
            bool wantsPounce = false;
            if (target != -1)
            {
                NPC n = Main.npc[target];
                float d = Vector2.Distance(Projectile.Center, n.Center);
                if (d < 70f && IsOnGround() && Math.Abs(Projectile.velocity.X) > 1.5f)
                    wantsPounce = true;
            }

            // localAI[1] 作为“飞扑剩余时间”(帧计数)
            if (wantsPounce && Projectile.localAI[1] <= 0f)
            {
                Projectile.localAI[1] = 18f; // 飞扑动画持续时长（你可调：12~24）
            }
            // ========== 动画：0~2待机(头右)，3~12奔跑(头左) ==========
            UpdateAnimation(onGround, target != -1);

            Projectile.rotation = 0f;

            // 注意：贴图里“待机帧头朝右、奔跑帧头朝左”，所以绘制翻转逻辑要分组处理（见 PreDraw）
        }

        private void UpdateAnimation(bool onGround, bool hasTarget)
        {
            // 计算是否在移动
            bool moving = Math.Abs(Projectile.velocity.X) > 0.35f;

            // ========= 飞扑动画优先（10~12）=========
            if (Projectile.localAI[1] > 0f)
            {
                Projectile.localAI[1]--;

                // 10~12 三帧，速度稍快一点
                if (Projectile.frame < 10 || Projectile.frame > 12)
                    Projectile.frame = 10;

                Projectile.frameCounter++;
                if (Projectile.frameCounter >= 4)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame > 12)
                        Projectile.frame = 10;
                }

                // 播完结束，回到奔跑起始帧
                if (Projectile.localAI[1] <= 0f)
                {
                    Projectile.frame = 3;
                    Projectile.frameCounter = 0;
                }

                return;
            }

            // ========= 待机（0~2）=========
            bool idle = !hasTarget && onGround && !moving;

            Projectile.frameCounter++;

            if (idle)
            {
                if (Projectile.frame < 0 || Projectile.frame > 2)
                    Projectile.frame = 0;

                if (Projectile.frameCounter >= 30)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame > 2)
                        Projectile.frame = 0;
                }
                return;
            }

            // ========= 正常奔跑（3~9）=========
            if (Projectile.frame < 3 || Projectile.frame > 9)
                Projectile.frame = 3;

            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 9)
                    Projectile.frame = 3;
            }
        }

        // ✅ 关键：因为你的贴图“待机帧默认朝右、奔跑帧默认朝左”，所以翻转要分开判断
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            int frameHeight = tex.Height / Main.projFrames[Type];
            // 左右边距5
            Rectangle src = new Rectangle(5, frameHeight * Projectile.frame, tex.Width - 10, frameHeight);

            Vector2 origin = src.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            bool idleGroup = Projectile.frame <= 2;

            // idle帧(头右)：想面向右 => 不翻转；想面向左 => 翻转
            // run帧(头左)：想面向左 => 不翻转；想面向右 => 翻转
            bool faceRight = Projectile.direction == 1;

            SpriteEffects fx;
            if (idleGroup)
                fx = faceRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            else
                fx = faceRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(tex, drawPos, src, lightColor, 0f, origin, 1f, fx, 0);
            return false;
        }

        // -----------------------
        // Targeting（沿用你蚱蜢那套思路，简化一点）
        // -----------------------
        private int FindTarget(Player owner, float maxDist)
        {
            // 1) 玩家手动锁定的NPC优先
            if (owner.HasMinionAttackTargetNPC)
            {
                int npcIndex = owner.MinionAttackTargetNPC;
                if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
                {
                    NPC n = Main.npc[npcIndex];
                    if (n.CanBeChasedBy(this) && Vector2.Distance(owner.Center, n.Center) <= maxDist)
                        return npcIndex;
                }
            }

            // 2) 自动找最近
            int best = -1;
            float bestDist = maxDist;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.CanBeChasedBy(this))
                    continue;

                float d = Vector2.Distance(Projectile.Center, n.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            return best;
        }

        // -----------------------
        // 简易地面判定
        // -----------------------
        private bool IsOnGround()
        {
            Rectangle feet = new Rectangle(
                (int)Projectile.position.X + 2,
                (int)(Projectile.position.Y + Projectile.height),
                Projectile.width - 4,
                4
            );

            if (Collision.SolidCollision(feet.Location.ToVector2(), feet.Width, feet.Height))
                return true;

            // 平台也算地面
            Point left = new Vector2(feet.Left, feet.Top).ToTileCoordinates();
            Point right = new Vector2(feet.Right, feet.Top).ToTileCoordinates();
            return IsPlatformTile(left) || IsPlatformTile(right);
        }

        private bool IsPlatformTile(Point p)
        {
            if (!WorldGen.InWorld(p.X, p.Y, 10))
                return false;

            Tile t = Main.tile[p.X, p.Y];
            if (t == null || !t.HasTile)
                return false;

            return TileID.Sets.Platforms[t.TileType];
        }

        // -----------------------
        // 前方障碍检测：用于“遇到墙/台阶就跳”
        // -----------------------
        private bool HasObstacleAhead(int dir)
        {
            // 在前方一点点的位置做一个小矩形探测
            int checkWidth = 10;
            int checkHeight = Projectile.height - 8;

            Vector2 checkPos = Projectile.position + new Vector2(dir == 1 ? Projectile.width : -checkWidth, 3f);
            return Collision.SolidCollision(checkPos, checkWidth, checkHeight);
        }

        private float Approach(float value, float target, float delta)
        {
            if (value < target)
                return MathHelper.Min(value + delta, target);
            if (value > target)
                return MathHelper.Max(value - delta, target);
            return value;
        }
    }
}
