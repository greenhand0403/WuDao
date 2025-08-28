using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Items.Pets
{
    public class DiscoBallRemote : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.noMelee = true;
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.shoot = ModContent.ProjectileType<DiscoBallPetProj>();
            Item.buffType = ModContent.BuffType<DiscoBallPetBuff>();
            Item.UseSound = SoundID.Item44; // 召唤宠物常用音效
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 360);
            }
        }
    }

    // 2) BUFF：标记为 LightPet，维持弹幕存在
    public class DiscoBallPetBuff : ModBuff
    {
        public override string Texture => $"WuDao/Common/Buffs/DiscoBallPetBuff";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.lightPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ModContent.ProjectileType<DiscoBallPetProj>());
        }
    }

    // 3) 宠物弹幕本体：参考猩红之心移动逻辑，加入“圆锥方向光 + 颜色循环 + 缓慢旋转”
    public class DiscoBallPetProj : ModProjectile
    {
        public override string Texture => $"WuDao/Content/Projectiles/DiscoBallPetProj";
        // 配置参数（可根据需要调节/做成 ModConfig）
        private const int BeamLengthTiles = 30;      // 期望照亮 45 格（~720 像素）
        private const int BeamStepTiles = 2;         // 取样步长（每 4 格一个采样点）
        private const float AmbientIntensity = 0.7f;// 环境光强度（0..1）
        private const float BeamIntensity = 4.2f;   // 主光束强度（会随距离衰减）
        private const int ColorHoldTicks = 35;      // 每种颜色保持
        private const float RotationSpeed = 0.015f;   // 自身缓慢旋转

        // 迪斯科舞厅常用的颜色（可自行微调/扩展）
        private static readonly Color[] DiscoColors = new[]
        {
            new Color(255,  60,  60), // Red
            new Color(255, 160,   0), // Orange/Amber
            new Color(255, 255,  60), // Yellow
            new Color( 60, 255,  60), // Green
            new Color( 60, 255, 255), // Cyan
            new Color( 60,  60, 255), // Blue
            new Color(180,  60, 255), // Purple
            new Color(255,  60, 180), // Magenta/Pink
        };

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.LightPet[Projectile.type] = true; // 告诉游戏这是照明宠物
            // 让宠物贴近玩家时更平滑
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, 1, 1);
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            AIType = 0;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                player.ClearBuff(ModContent.BuffType<DiscoBallPetBuff>());
                return;
            }
            if (player.HasBuff(ModContent.BuffType<DiscoBallPetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            // ---- (A) 上方&左右轻微移动 ----
            // 头顶基准高度：略高于玩家 64px，可按需求调
            float baseHeight = 48f;
            // 左右摆动：幅度与速度可调
            float swayAmp = 40f;
            float swaySpeed = 0.03f; // 越大摆动越快
            float swayX = (float)System.Math.Sin(Main.GameUpdateCount * swaySpeed) * swayAmp;

            // 轻微上下漂浮（可选）
            float bobAmp = 6f;
            float bobSpeed = 0.02f;
            float bobY = (float)System.Math.Sin(Main.GameUpdateCount * bobSpeed) * bobAmp;

            Vector2 target = player.Center + new Vector2(swayX, -baseHeight + bobY);

            // 惯性跟随：平滑靠近目标点
            float speed = 30f;     // 最大趋近速度
            float inertia = 15f;   // 惯性（数值越大越平滑）
            Vector2 toTarget = target - Projectile.Center;
            if (toTarget.Length() > speed)
                toTarget = toTarget.SafeNormalize(Vector2.UnitY) * speed;

            Projectile.velocity = (Projectile.velocity * (inertia - 1f) + toTarget) / inertia;

            // 让转向更自然（仅用于朝向/美术，不影响逻辑）
            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.direction = Projectile.spriteDirection =
                    (Projectile.velocity.X >= 0f) ? 1 : -1;

            // 自转决定主光束方向（保留）
            Projectile.rotation += RotationSpeed;
            Vector2 beamDir = Projectile.rotation.ToRotationVector2();

            // —— 颜色直接跳色 —— //
            int total = DiscoColors.Length;
            int seg = (int)(Main.GameUpdateCount / ColorHoldTicks) % total;
            Color cur = DiscoColors[seg];

            // —— 环境光：每帧都加（便宜） —— //
            Vector3 cv3 = cur.ToVector3();
            Lighting.AddLight(Projectile.Center, cv3 * AmbientIntensity);

            // —— 主光束照明：每 3 帧执行一次，省 CPU —— //
            if (Main.GameUpdateCount % 3 == 0)
            {
                int samples = BeamLengthTiles / BeamStepTiles; // 30/2=15 次左右
                float stepPixels = BeamStepTiles * 16f;

                for (int i = 1; i <= samples; i++)
                {
                    // 轴向点（无抖动/无侧瓣）
                    Vector2 pos = Projectile.Center + beamDir * stepPixels * i;

                    // 距离衰减（简单平方）
                    float dist01 = i / (float)samples;   // 0..1
                    float fall = 1f - dist01;
                    fall *= fall;

                    Vector3 beam = cv3 * (BeamIntensity * fall);
                    Lighting.AddLight(pos, beam);
                }
            }
        }
    }
}
