using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Wudao.Content.Items.Pets
{
    // 1) 物品：用于召唤照明宠物
    public class DiscoBallRemote : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.GolfBall}";
        
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
            if (player.whoAmI==Main.myPlayer && player.itemTime==0)
            {
                player.AddBuff(Item.buffType, 360);
            }
        }
    }

    // 2) BUFF：标记为 LightPet，维持弹幕存在
    public class DiscoBallPetBuff : ModBuff
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.GolfBall}";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Disco Ball");
            // Description.SetDefault("A disco ball follows you, lighting up the dance floor!");
            Main.buffNoTimeDisplay[Type] = true;
            // Main.buffNoSave[Type] = true;
            Main.lightPet[Type] = true;
            // tML 1.4 标记 Light Pet 的推荐做法
            // BuffID.Sets.IsLightPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex,ref unused,ModContent.ProjectileType<DiscoBallPetProj>());
        }
    }

    // 3) 宠物弹幕本体：参考猩红之心移动逻辑，加入“圆锥方向光 + 颜色循环 + 缓慢旋转”
    public class DiscoBallPetProj : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.GolfBall}";
        // 配置参数（可根据需要调节/做成 ModConfig）
        private const int BeamLengthTiles = 60;      // 期望照亮 60 格（~960 像素）
        private const int BeamStepTiles = 4;         // 取样步长（每 4 格一个采样点）
        private const float AmbientIntensity = 0.55f;// 环境光强度（0..1）
        private const float BeamIntensity = 1.15f;   // 主光束强度（会随距离衰减）
        private const int ColorHoldTicks = 45;      // 每种颜色保持 3 秒（60fps）
        private const float RotationSpeed = 0.045f;   // 自身缓慢旋转

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
            // Projectile.CloneDefaults(ProjectileID.CrimsonHeart); // 复用移动/跟随 AI
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            // AIType = ProjectileID.CrimsonHeart; // 参考猩红之心
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
            float baseHeight = 64f;
            // 左右摆动：幅度与速度可调
            float swayAmp = 40f;
            float swaySpeed = 0.05f; // 越大摆动越快
            float swayX = (float)System.Math.Sin(Main.GameUpdateCount * swaySpeed) * swayAmp;

            // 轻微上下漂浮（可选）
            float bobAmp = 6f;
            float bobSpeed = 0.03f;
            float bobY = (float)System.Math.Sin(Main.GameUpdateCount * bobSpeed) * bobAmp;

            Vector2 target = player.Center + new Vector2(swayX, -baseHeight + bobY);

            // 惯性跟随：平滑靠近目标点
            float speed = 50f;     // 最大趋近速度
            float inertia = 10f;   // 惯性（数值越大越平滑）
            Vector2 toTarget = target - Projectile.Center;
            if (toTarget.Length() > speed)
                toTarget = toTarget.SafeNormalize(Vector2.UnitY) * speed;

            Projectile.velocity = (Projectile.velocity * (inertia - 1f) + toTarget) / inertia;

            // 让转向更自然（仅用于朝向/美术，不影响逻辑）
            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.direction = Projectile.spriteDirection =
                    (Projectile.velocity.X >= 0f) ? 1 : -1;

            // 自身缓慢自转，用于主光束方向
            Projectile.rotation += RotationSpeed;
            Vector2 beamDir = Projectile.rotation.ToRotationVector2();

            // ---- (B) 颜色平滑过渡 ----
            int total = DiscoColors.Length;
            int seg = (int)(Main.GameUpdateCount / ColorHoldTicks) % total;
            // int next = (seg + 1) % total;
            // float t = (Main.GameUpdateCount % ColorHoldTicks) / (float)ColorHoldTicks; // 0..1
            // Color cur = Color.Lerp(DiscoColors[seg], DiscoColors[next], t);
            // t: 0..1，过渡只占前 15%，其余时间保持纯色
            // float blendWindow = 0.15f;
            // float tt = t < blendWindow ? (t / blendWindow) : 1f;
            // Color cur = Color.Lerp(DiscoColors[seg], DiscoColors[next], tt);

            Color cur = DiscoColors[seg];

            // ---- (C) 环境光 + 定向扫光 ----
            // 新增：亮度脉冲（0.85~1.25 之间摆动）
            float pulse = 0.85f + 0.40f * (0.5f + 0.5f * (float)System.Math.Sin(Main.GameUpdateCount * 0.25f));
            // 也可以把 0.25f 提到 0.6f 做更“快节奏”的闪烁

            Vector3 ambient = cur.ToVector3() * (AmbientIntensity * pulse);
            Lighting.AddLight(Projectile.Center, ambient);

            int samples = BeamLengthTiles / BeamStepTiles;
            float stepPixels = BeamStepTiles * 16f;
            float coneHalfAngle = MathHelper.ToRadians(18f);

            for (int i = 1; i <= samples; i++)
            {
                Vector2 pos = Projectile.Center + beamDir * stepPixels * i;

                float dist01 = i / (float)samples;
                float distFalloff = 1f - dist01;
                distFalloff *= distFalloff;

                // 旧：0.07f 与 6f
                float wobble = (float)System.Math.Sin((Main.GameUpdateCount + i * 12) * 0.12f) * 10f;
                pos += beamDir.RotatedBy(MathHelper.PiOver2) * wobble;

                Vector3 beam = cur.ToVector3() * (BeamIntensity * distFalloff * pulse);
                Lighting.AddLight(pos, beam);

                const int sideRays = 2;
                for (int s = 1; s <= sideRays; s++)
                {
                    float frac = s / (float)(sideRays + 1);
                    float angle = MathHelper.Lerp(0f, coneHalfAngle, frac);

                    Vector2 dirL = beamDir.RotatedBy(-angle);
                    Vector2 dirR = beamDir.RotatedBy(angle);
                    Vector2 posL = Projectile.Center + dirL * stepPixels * i;
                    Vector2 posR = Projectile.Center + dirR * stepPixels * i;

                    float sideFalloff = distFalloff * (1f - frac * 0.6f);
                    Vector3 side = cur.ToVector3() * (BeamIntensity * 0.6f * sideFalloff * pulse);

                    Lighting.AddLight(posL, side);
                    Lighting.AddLight(posR, side);
                }
            }
        }

        // 选做：在地图上也展示彩色小点（可注释掉）
        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
