using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles.Summon
{
    public class MonsterTrainMinion : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.BeeMinecart}";
        private const float OrbitRadius = 120f;
        private const float OrbitAngularSpeed = 0.045f;

        private const float FollowLerp = 0.12f;

        private const float DashTriggerDist = 520f;
        private const float DashSpeed = 18f;
        private const int DashTime = 22;

        private const int ShootCooldown = 40; // 约 0.67 秒
        private const float ShootSpeed = 10f;

        private static readonly int PygmyBase = 191; // 191~194
        private const int PygmyFrames = 18;
        private const int PygmyFrameW = 50;
        private const int PygmyFrameH = 48; // 864/18
        public override bool? CanCutTiles() => false;
        private const float TrainSpacing = 38f;     // 车厢间距，按矿车贴图调
        private const float TrainHardness = 0.25f;  // 位置纠正强度（越大越“硬”）
        private const float TrainLerp = 0.22f;      // 速度平滑（越大越跟手）
                                                    // === Hard Train Movement (no rotation) ===
        private const float MaxSpeedX = 16f;   // 横向快
        private const float MaxSpeedY = 4.5f;  // 纵向慢

        private const float AccelX = 0.35f;
        private const float AccelY = 0.10f;

        private const float ShuttleFarX = 420f;    // 敌人左右穿梭的“远距离”
        private const float FlipThreshold = 40f;   // 接近目标侧就翻向
        private const float TurnNudgeY = 2.8f;     // 翻向时上下抖一下

        private const float IdleFarX = 240f; // 无敌人时相对玩家左右偏移
        private const float IdleY = -30f;    // 无敌人时相对玩家高度

        private const float CarSpacing = 38f;  // 车厢间距（你原来 TrainSpacing=38 可以直接复用）
        // 放在类内部（字段区域）
        private static readonly int[] MinecartItemIDs = new int[]
        {
            ItemID.Minecart,              // 0 普通矿车
            ItemID.MinecartMech,    // 1 机械矿车
            ItemID.DesertMinecart,       // 2 沙漠矿车
            ItemID.FishMinecart,          // 3 鲤鱼矿车
            ItemID.MeowmereMinecart,          // 4 彩虹猫矿车
            ItemID.BeeMinecart,           // 5 蜜蜂矿车
            ItemID.FartMinecart,          // 6 放屁矿车
            ItemID.SteampunkMinecart      // 7 蒸汽矿车
        };
        private enum AIState : int
        {
            Orbit = 0,
            Dash = 1
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 22;

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;

            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            // 接触伤害节奏更稳定
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
        }
        private int GetTrainCount()
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active) continue;
                if (p.owner != Projectile.owner) continue;
                if (p.type != Projectile.type) continue;
                count++;
            }
            return Math.Clamp(count, 1, 8);
        }

        private float GetTrainDamageMult()
        {
            int count = GetTrainCount();
            return 1f + 0.08f * (count - 1); // 你可调 0.05~0.10
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage *= GetTrainDamageMult();
        }
        private void DoTrainCarFollow(int carIndex)
        {
            Projectile prev = FindTrainCar(carIndex - 1);
            if (prev == null)
            {
                prev = FindTrainCar(0);
                if (prev == null) return;
            }

            // 前车尾巴点：由前车 spriteDirection 决定尾巴在左/右
            Vector2 tailDir = (prev.spriteDirection == 1) ? Vector2.UnitX : -Vector2.UnitX;
            Vector2 targetPos = prev.Center - tailDir * CarSpacing;

            Vector2 to = targetPos - Projectile.Center;

            // 横快竖慢（跟随时也如此）
            float desiredVx = Math.Clamp(to.X * 0.12f, -MaxSpeedX, MaxSpeedX);
            float desiredVy = Math.Clamp(to.Y * 0.10f, -MaxSpeedY, MaxSpeedY);

            Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, desiredVx, 0.40f);
            Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, desiredVy, 0.14f);

            Projectile.rotation = 0f;
            Projectile.spriteDirection = Projectile.direction = prev.spriteDirection;
        }

        private Projectile FindTrainCar(int index)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active) continue;
                if (p.owner != Projectile.owner) continue;
                if (p.type != Projectile.type) continue;
                if ((int)p.ai[2] != index) continue;
                return p;
            }
            return null;
        }
        public override void AI()
        {
            // owner 安全检查
            int ownerIndex = Projectile.owner;
            if (ownerIndex < 0 || ownerIndex >= Main.player.Length)
            {
                Projectile.Kill();
                return;
            }

            Player owner = Main.player[ownerIndex];
            if (!owner.active || owner.dead)
            {
                owner.ClearBuff(ModContent.BuffType<MonsterTrainBuff>());
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<MonsterTrainBuff>()))
                Projectile.timeLeft = 2;

            NPC target = FindTarget(owner, 900f);

            int carIndex = (int)Projectile.ai[2];

            // 运动：车头/车厢分工
            if (carIndex == 0)
                UpdateHeadMovement(owner, target);
            else
                DoTrainCarFollow(carIndex);

            // 全车都能射矛（你要这样）
            if (target != null)
                TryShoot(owner, target);

            // === 矮人朝向与动画（对所有车都生效）===
            // 矿车不旋转：只左右翻，方向来自水平速度（或继承前车）
            int cartDir = Projectile.spriteDirection;
            int riderDir = cartDir;

            // 有敌人且正在投矛动作窗口时：矮人转身朝敌人
            if (Projectile.frameCounter > 0 && target != null && target.active)
                riderDir = (target.Center.X >= Projectile.Center.X) ? 1 : -1;

            // frame: 0=朝左，1=朝右（仅用于绘制 riderFx）
            Projectile.frame = (riderDir == 1) ? 1 : 0;

            if (Projectile.frameCounter > 0)
                Projectile.frameCounter--;
        }
        private void UpdateHeadMovement(Player owner, NPC target)
        {
            // ai[0] 用作 shuttleDir（+1/-1），ai[1] 用作 nudgeDir（+1/-1）
            if (Projectile.ai[0] == 0f)
                Projectile.ai[0] = 1f;
            if (Projectile.ai[1] == 0f)
                Projectile.ai[1] = 1f;

            Vector2 anchor = (target != null) ? target.Center : owner.Center;

            float farX = (target != null) ? ShuttleFarX : IdleFarX;
            float wantX = anchor.X + Projectile.ai[0] * farX;
            float wantY = (target != null) ? anchor.Y : (anchor.Y + IdleY);

            // 有敌人：左右来回穿梭 + 硬核转向“抖一下”
            if (target != null)
            {
                float dxToWant = wantX - Projectile.Center.X;

                bool nearSide = Math.Abs(dxToWant) < FlipThreshold;
                bool passedSide = (Projectile.ai[0] > 0f && Projectile.Center.X > wantX) ||
                                  (Projectile.ai[0] < 0f && Projectile.Center.X < wantX);

                if (nearSide || passedSide)
                {
                    Projectile.ai[0] *= -1f;   // 翻向
                    Projectile.ai[1] *= -1f;   // 抖动方向也翻一下
                    Projectile.velocity.Y = Projectile.ai[1] * TurnNudgeY;
                }

                // 翻向后更新 wantX
                wantX = anchor.X + Projectile.ai[0] * farX;
            }

            // 横快竖慢追踪
            float dx = wantX - Projectile.Center.X;
            float dy = wantY - Projectile.Center.Y;

            float desiredVx = Math.Clamp(dx * 0.08f, -MaxSpeedX, MaxSpeedX);
            float desiredVy = Math.Clamp(dy * 0.06f, -MaxSpeedY, MaxSpeedY);

            Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, desiredVx, AccelX);
            Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, desiredVy, AccelY);

            // 不旋转
            Projectile.rotation = 0f;

            // 贴图朝向：跟随水平运动方向
            if (Math.Abs(Projectile.velocity.X) > 0.05f)
                Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X >= 0f) ? 1 : -1;
        }
        private void SwitchState(AIState newState)
        {
            Projectile.ai[0] = (float)newState;
            Projectile.ai[1] = 0f;
            Projectile.netUpdate = true;
        }

        private NPC FindTarget(Player owner, float maxRange)
        {
            // 1) 玩家手动指定目标（召唤锁定）
            int forced = owner.MinionAttackTargetNPC;
            if (forced >= 0 && forced < Main.maxNPCs)
            {
                NPC npc = Main.npc[forced];
                if (npc.CanBeChasedBy(this) && Vector2.Distance(owner.Center, npc.Center) <= maxRange)
                    return npc;
            }

            // 2) 自动找最近
            NPC best = null;
            float bestDist = maxRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this)) continue;

                float d = Vector2.Distance(Projectile.Center, npc.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = npc;
                }
            }

            return best;
        }

        private void TryShoot(Player owner, NPC target)
        {
            // localAI[1] = shoot timer
            Projectile.localAI[1]++;

            if (Projectile.localAI[1] < ShootCooldown)
                return;

            Projectile.localAI[1] = 0f;

            if (Main.myPlayer != Projectile.owner)
                return;

            // ✅ 从矮人位置发射（与绘制 offset 对齐）
            Vector2 from = Projectile.Center + new Vector2(0f, -24f);
            Vector2 to = target.Center - from;

            if (to.LengthSquared() < 16f) return;
            to.Normalize();

            Vector2 vel = to * ShootSpeed;
            int dmg = (int)(Projectile.damage * GetTrainDamageMult());
            // 原版矮人长矛弹幕：ProjectileID.PygmySpear
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                from,
                vel,
                ModContent.ProjectileType<MonsterTrainSpear>(),
                dmg,
                Projectile.knockBack,
                Projectile.owner
            );

            Projectile.frameCounter = 18; // 触发投矛动作（可调 12~24）
        }
        // 获取当前矿车索引
        private int GetMinecartIndex()
        {
            int index = (int)Projectile.ai[2];

            if (index < 0)
                index = 0;

            if (index >= MinecartItemIDs.Length)
                index = MinecartItemIDs.Length - 1;

            return index;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            // 矿车：默认朝右
            SpriteEffects cartFx = (Projectile.spriteDirection == -1)
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            // 矮人：默认朝左（frame=1 表示朝右）
            SpriteEffects riderFx = (Projectile.frame == 1)
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            DrawFullPygmy(sb, pos, lightColor, riderFx);        // ①先画矮人
            DrawMinecartByIndex(sb, pos, lightColor, cartFx);  // ②再画矿车遮挡
            return false;
        }

        private void DrawFullPygmy(SpriteBatch sb, Vector2 cartScreenPos, Color lightColor, SpriteEffects riderFx)
        {
            int minecartIndex = GetMinecartIndex();
            int pygmyType = 191 + (minecartIndex % 4);

            Main.instance.LoadProjectile(pygmyType);
            Texture2D tex = TextureAssets.Projectile[pygmyType].Value;

            int frames = Main.projFrames[pygmyType];
            if (frames <= 0) frames = 1;

            int frameH = tex.Height / frames;

            // 4帧逻辑：待机=0；投矛=1..3（frameCounter>0 表示正在投矛窗口）
            int frameIndex;
            if (Projectile.frameCounter > 0 && frames >= 4)
            {
                int anim = (int)(Main.GameUpdateCount / 6 % 3);
                frameIndex = 1 + anim;
            }
            else
            {
                frameIndex = 0;
            }
            frameIndex = Math.Clamp(frameIndex, 0, frames - 1);

            Rectangle source = new Rectangle(0, frameIndex * frameH, tex.Width, frameH);

            Vector2 offset = new Vector2(0f, -24f);
            Vector2 origin = source.Size() * 0.5f;

            // ✅ 不旋转
            sb.Draw(tex, cartScreenPos + offset, source, lightColor, 0f, origin, 1f, riderFx, 0f);
        }

        private void DrawMinecartByIndex(SpriteBatch sb, Vector2 pos, Color lightColor, SpriteEffects fx)
        {
            int index = GetMinecartIndex();
            if (index < 0) index = 0;
            if (index >= MinecartItemIDs.Length) index = MinecartItemIDs.Length - 1;

            int itemID = MinecartItemIDs[index];

            Main.instance.LoadItem(itemID);
            Texture2D tex = TextureAssets.Item[itemID].Value;

            Vector2 origin = tex.Size() * 0.5f;

            // ✅ 不旋转
            sb.Draw(tex, pos, null, lightColor, 0f, origin, 1f, fx, 0f);
        }
    }
}