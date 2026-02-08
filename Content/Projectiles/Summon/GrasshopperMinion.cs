using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles.Summon
{
    public class GrasshopperMinion : ModProjectile
    {
        // 复用原版蚱蜢 NPC 贴图（18x24，共2帧，垂直）
        public override string Texture => "Terraria/Images/NPC_" + NPCID.Grasshopper;

        /*
         * ai/localAI 约定：
         * ai[0]      跳跃冷却（ticks）
         * ai[1]      当前目标NPC索引（-1表示无）
         * localAI[0] 是否处于“赶路模式”(1=赶路，0=正常)
         * localAI[1] 卡住计时（位置几乎不变时累计）
         */

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 2; // 两帧
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            Main.projPet[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 10; // /2 取1帧 -2 贴地

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.DamageType = DamageClass.Summon;

            // 接触伤害 + 本地无敌帧
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override bool MinionContactDamage() => true;
        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // 维持buff（经典写法）
            if (owner.HasBuff(ModContent.BuffType<GrasshopperBuff>()))
                Projectile.timeLeft = 2;

            // 待机点：玩家身后一点
            Vector2 idlePos = owner.Center + new Vector2(owner.direction * -50f, -20f);

            // 目标
            int target = FindTarget(owner, 650f);
            Projectile.ai[1] = target;

            // 距离玩家
            float distToOwner = Vector2.Distance(Projectile.Center, owner.Center);

            // 如果非常远，兜底瞬移（避免永远迷路）
            if (distToOwner > 1800f)
            {
                Projectile.Center = owner.Center;
                Projectile.velocity = Vector2.Zero;
                Projectile.localAI[0] = 0f;
                Projectile.localAI[1] = 0f;
                Projectile.netUpdate = true;
                return;
            }

            // 卡住检测（走不过地形就会触发）
            bool stuck = UpdateStuckCounter();

            // 触发赶路模式：离玩家较远或卡住太久
            if (distToOwner > 750f || stuck)
                Projectile.localAI[0] = 1f;

            // ========== 赶路模式：tileCollide=false 飞回玩家附近 ==========
            if (Projectile.localAI[0] == 1f)
            {
                Projectile.tileCollide = false;

                Vector2 toIdle = idlePos - Projectile.Center;
                float len = toIdle.Length();

                // 到附近就退出赶路
                if (len < 50f)
                {
                    Projectile.localAI[0] = 0f;
                    Projectile.localAI[1] = 0f;
                    Projectile.tileCollide = true;
                    Projectile.velocity *= 0.2f;
                    Projectile.netUpdate = true;
                }
                else
                {
                    toIdle /= len;

                    float flySpeed = 10f; // 赶路速度
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle * flySpeed, 0.14f);
                }

                // 朝向（用于绘制）
                if (Projectile.velocity.X > 0.15f)
                    SetFacingRight();
                else if (Projectile.velocity.X < -0.15f)
                    SetFacingLeft();

                // 动画：赶路时基本在空中帧更自然
                Projectile.frame = 1;

                return;
            }

            // ========== 正常模式：贴地跳 ==========
            Projectile.tileCollide = true;

            bool onGround = IsOnGround();

            // 动画：地面0帧，空中1帧（符合原版蚱蜢）
            Projectile.frame = onGround ? 0 : 1;

            // 重力 + 最大下落
            if (!onGround)
            {
                Projectile.velocity.Y += 0.45f;
                if (Projectile.velocity.Y > 12f)
                    Projectile.velocity.Y = 12f;
            }
            else
            {
                // “压地”减少漂浮感（平台/半砖更明显）
                if (Projectile.velocity.Y > 0f)
                    Projectile.velocity.Y = 0.10f;
            }

            // 跳跃冷却递减
            if (Projectile.ai[0] > 0f)
                Projectile.ai[0]--;

            // 目的地：有目标去目标；没目标回玩家
            Vector2 dest = (target != -1) ? Main.npc[target].Center : idlePos;

            // 朝向目的地（用于移动+绘制）
            if (dest.X > Projectile.Center.X)
                SetFacingRight();
            else
                SetFacingLeft();

            float distToDest = Vector2.Distance(Projectile.Center, dest);

            // ——像原版蚱蜢：落地会停顿一下，再下一跳——
            // 近距离待机就别乱跳
            if (target == -1 && distToDest < 70f)
            {
                Projectile.velocity.X *= 0.80f;
                return;
            }

            // 只在地面发起跳跃
            if (onGround)
            {
                // 让它不“滑行”，更像虫子停一下再跳
                Projectile.velocity.X *= 0.90f;

                if (Projectile.ai[0] <= 0f)
                {
                    // “小跳频繁”
                    float hopXSpeed = (target != -1) ? 4.2f : 3.6f;
                    float hopYSpeed = (target != -1) ? 7.0f : 6.4f;

                    // 起跳
                    Projectile.velocity.X = Projectile.direction * hopXSpeed;
                    Projectile.velocity.Y = -hopYSpeed;

                    // 跳跃间隔：越小越频繁（建议 14~22）
                    // 这里 18 比较接近“短跳+停顿”的感觉
                    Projectile.ai[0] = 18f;

                    Projectile.netUpdate = true;
                }
            }
            else
            {
                // 空中：不要强行横向加速，否则会像漂浮
                Projectile.velocity.X *= 0.985f;
            }

            Projectile.rotation = 0f;
        }

        // -----------------------
        // 朝向控制（关键：NPC贴图用Projectile时建议spriteDirection取反）
        // 这里我们统一：Projectile.direction 表示“运动/逻辑方向”
        // 绘制时用 spriteDirection 来决定 Flip（并在PreDraw里实现）
        // -----------------------
        private void SetFacingRight()
        {
            Projectile.direction = 1;

            // ✅ NPC贴图默认多为朝左，给Projectile用时通常要取反才符合直觉
            Projectile.spriteDirection = -1; // 让它“面向右”
        }

        private void SetFacingLeft()
        {
            Projectile.direction = -1;
            Projectile.spriteDirection = 1;  // 保持默认朝左
        }

        // -----------------------
        // 更稳的“是否在地面”判定：脚下4像素区域碰撞 + 平台判定
        // -----------------------
        private bool IsOnGround()
        {
            Rectangle feet = new Rectangle(
                (int)Projectile.position.X + 2,
                (int)(Projectile.position.Y + Projectile.height),
                Projectile.width - 4,
                4 // ✅ 4像素更稳，减少“站地上却被判空中”的闪帧
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
        // 卡住检测：位置几乎不动就计时，超过阈值触发赶路
        // -----------------------
        private bool UpdateStuckCounter()
        {
            Vector2 old = (Projectile.oldPos != null && Projectile.oldPos.Length > 0) ? Projectile.oldPos[0] : Projectile.position;
            float moved = Vector2.Distance(old, Projectile.position);

            if (moved < 0.45f)
                Projectile.localAI[1]++;
            else
                Projectile.localAI[1] = 0f;

            // ~1.5秒（60fps下90ticks）认为卡住
            return Projectile.localAI[1] > 90f;
        }

        // -----------------------
        // 索敌：优先鞭子目标，其次最近敌怪
        // -----------------------
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
                if (!npc.CanBeChasedBy(this))
                    continue;

                float d = Vector2.Distance(Projectile.Center, npc.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            return best;
        }

        // -----------------------
        // ✅ 绘制：手动PreDraw，彻底保证翻转正确（修复你说的“永远不翻”）
        // -----------------------
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle src = new Rectangle(0, Projectile.frame * frameHeight, tex.Width, frameHeight);

            // Projectile.spriteDirection == -1 => 翻转
            SpriteEffects fx = (Projectile.spriteDirection == -1)
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            Vector2 origin = src.Size() * 0.5f;

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                src,
                lightColor,
                0f,
                origin,
                Projectile.scale,
                fx,
                0
            );

            return false;
        }
    }
}