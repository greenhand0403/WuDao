using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles
{
    // 丘比宠物射弹主体
    public class KyubeyPetProjectile : ModProjectile
    {
        // 状态：近走远飞（像宠物兔子那套）
        private enum PetState { Walk = 0, Fly = 1 }
        private PetState State
        {
            get => (PetState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }
        // 帧：0~5 为“奔跑循环”；静止用第4帧；飞行用第1帧
        // 注意：代码里是 0-based：第1帧→索引0；第4帧→索引3
        private const int RunStart = 0, RunCount = 6;
        private const int IdleFrame = 3; // 第4帧
        private const int FlyFrame = 0;  // 第1帧
        private bool runFlag = false;
        private int frameSpeed = 6;      // 奔跑帧速，6=每6tick切一帧

        // 切换距离（带回滞），以及移动参数
        private const float FlyEnterDist = 260f, FlyExitDist = 180f; // 远了飞、近了走
        private const float WalkRunSpeed = 3.0f, WalkAccel = 0.12f;
        private const float Gravity = 0.15f, MaxFall = 6.0f;
        private int stuckCounter = 0;
        private const int BlockedToFlyTicks = 10; // 被挡 >= 10tick 才切飞，防抖

        public override string Texture => "WuDao/Content/Items/Pets/Kyubey";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = RunCount;  // ← 使用我们刚做好的 6 帧精灵表
            Main.projPet[Projectile.type] = true;

            // 人物选择界面的宠物预览（静态展示用，不跑 AI）
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] =
                ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], frameSpeed)
                    .WithOffset(-10, -20f)     // 预览相对位置，可按需要微调
                    .WithSpriteDirection(1)   // 朝向
                    .WithCode(DelegateMethods.CharacterPreview.Float); // 浮空效果
        }

        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;     // 近身走路需要地面碰撞
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;           // <—— 关键：完全走自定义 AI
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 保活（和你现有写法一致）
            if (!player.dead && player.HasBuff(ModContent.BuffType<KyubeyPetBuff>()))
                Projectile.timeLeft = 2;

            float dist = Vector2.Distance(player.Center, Projectile.Center);

            // —— 状态切换（远飞近走，带回滞）——
            if (State == PetState.Fly && dist < FlyExitDist) State = PetState.Walk;
            if (State == PetState.Walk && dist > FlyEnterDist) State = PetState.Fly;

            if (State == PetState.Fly)
            {
                // —— 飞行追踪：更快、更灵活、更会“往上爬” —— 
                Projectile.tileCollide = false;

                // 1) 锚点 = 玩家身侧上方 + 速度预判（水平/垂直都跟随得更紧）
                Vector2 baseOffset = new Vector2(-40 * player.direction, -24f);
                Vector2 anchor = player.Center + baseOffset + player.velocity * 10f; // 10tick 领航

                // 2) 若明显在玩家下方，额外“抬高目标点”
                float yBelow = player.Center.Y - Projectile.Center.Y; // >0 表示宠物在下
                if (yBelow > 32f)
                    anchor.Y -= Math.Min(56f, yBelow * 0.45f); // 落差越大，目标越高

                // 3) 距离自适应 速度/惯性（远→快/灵，近→稳）
                float distToAnchor = Vector2.Distance(Projectile.Center, anchor);
                float speed = distToAnchor > 600f ? 14f : (distToAnchor > 300f ? 10f : 7.5f);
                float inertia = distToAnchor > 600f ? 10f : (distToAnchor > 300f ? 14f : 18f);

                // 4) 额外上升助推（尤其是在玩家上方平台/半砖之类时）
                if (Projectile.Center.Y > player.Center.Y - 12f)
                    Projectile.velocity.Y -= 0.35f; // 微弱持续上抬，帮助快速“爬高”

                // 5) 常规插值逼近
                Vector2 desired = anchor - Projectile.Center;
                if (desired.Length() > 6f)
                {
                    desired.Normalize();
                    desired *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desired) / inertia;
                }
                else
                {
                    Projectile.velocity *= 0.90f; // 到位悬停
                }

                // 6) 超远兜底：卡很远或被卡体素时直接拉回
                if (distToAnchor > 1600f || !player.active)
                {
                    Projectile.Center = player.Center + new Vector2(-16 * player.direction, -24f);
                    Projectile.velocity = Vector2.Zero;
                }

                // 动画：飞行固定第1帧（索引0）
                Projectile.frameCounter = 0;
                Projectile.frame = FlyFrame;
                runFlag = false; // 你的 PostAI 会用到
            }
            else
            {
                // —— Walk —— 
                Projectile.tileCollide = true;

                // 重力（保持贴地）
                if (Projectile.velocity.Y < MaxFall)
                    Projectile.velocity.Y += Gravity;

                // —— 停止带宽 + 摩擦 —— 
                float dx = player.Center.X - Projectile.Center.X;
                const float StopXDeadzone = 22f;
                const float StopFriction = 0.86f;
                const float ZeroVelEps = 0.08f;

                if (Math.Abs(dx) <= StopXDeadzone)
                {
                    Projectile.velocity.X *= StopFriction;
                    if (Math.Abs(Projectile.velocity.X) < ZeroVelEps)
                        Projectile.velocity.X = 0f;
                }
                else
                {
                    float dir = Math.Sign(dx);
                    Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, dir * WalkRunSpeed, WalkAccel);
                }

                // —— “被地形挡住”检测：脚边前探一小段，如果前面是实心，累积卡住计数 —— 
                int dirTile = Math.Abs(dx) <= 0.01f ? Projectile.direction : Math.Sign(dx);
                // 在脚边前方 8px 处，用整宽度 6px 高度探测是否实心
                bool onGround = Collision.SolidCollision(
                    new Vector2(Projectile.position.X, Projectile.position.Y + Projectile.height + 2f),
                    Projectile.width, 4);

                bool blockedAhead = false;
                if (onGround)
                {
                    Rectangle ahead = new Rectangle(
                        (int)(Projectile.position.X + dirTile * 8f),
                        (int)(Projectile.position.Y + Projectile.height - 12f),
                        Projectile.width, 12);
                    blockedAhead = Collision.SolidCollision(new Vector2(ahead.X, ahead.Y), ahead.Width, ahead.Height);
                }

                if (blockedAhead)
                {
                    if (++stuckCounter >= BlockedToFlyTicks)
                    {
                        // 不需要跳，直接切飞行跨越障碍
                        State = PetState.Fly;
                        stuckCounter = 0;
                    }
                }
                else
                {
                    stuckCounter = 0;
                }

                // —— 模拟奔跑 —— 
                if (Math.Abs(dx) > frameSpeed)
                {
                    // 推进奔跑帧
                    runFlag = true;
                    Projectile.frameCounter++;
                    if (Projectile.frameCounter >= frameSpeed)
                    {
                        Projectile.frameCounter = 0;
                        Projectile.frame++;
                        if (Projectile.frame >= RunStart + RunCount)
                            Projectile.frame = RunStart;
                    }
                }
                else
                {
                    // 静止时播放 Idle 帧（第4帧）
                    runFlag = false;
                    Projectile.frameCounter = 0;
                    Projectile.frame = IdleFrame;
                }
            }
        }

        public override void PostAI()
        {
            // 根据速度/方向微调朝向（可选）
            if (Projectile.velocity.X != 0 && runFlag)
            {
                Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
            }
            else
            {
                Projectile.spriteDirection = Main.LocalPlayer.direction;
            }
        }
    }
}