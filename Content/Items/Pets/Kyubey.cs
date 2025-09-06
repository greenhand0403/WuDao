using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Items.Pets
{
    // 1) Buff：显示为虚饰宠物，维持时间并保证召唤
    public class KyubeyPetBuff : ModBuff
    {
        public override string Texture => "WuDao/Content/Items/Pets/KyubeySoulStone";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; // 不显示计时
            Main.vanityPet[Type] = true;         // 虚饰/宠物 Buff
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            // 如果需要则生成宠物，并将 buff 时间维持在 2 tick（持续刷新）
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused,
                ModContent.ProjectileType<KyubeyPetProjectile>());
        }
    }

    // 2) 召唤物品：使用后给自己添加 Buff，并让 Buff 负责生成宠物
    public class Kyubey : ModItem
    {
        public override string Texture => "WuDao/Content/Items/Pets/KyubeySoulStone";
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish); // 复制微风鱼的物品属性
            Item.shoot = ModContent.ProjectileType<KyubeyPetProjectile>(); // “发射”宠物
            Item.buffType = ModContent.BuffType<KyubeyPetBuff>();          // 使用时添加的 Buff
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.AddBuff(Item.buffType, 3600); // 1 分钟；实际会被 Update 里维持住
            }
            return true;
        }

        // 如需合成，可在这里添加配方
        // public override void AddRecipes() { ... }
    }

    // 3) 宠物射弹主体：帧动画6帧，AI仿微风鱼
    public class KyubeyPetProjectile : ModProjectile
    {
        private bool runFlag;
        private int frameSpeed = 12;
        public override string Texture => "WuDao/Content/Items/Pets/Kyubey";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 11;  // ← 使用我们刚做好的 6 帧精灵表
            Main.projPet[Projectile.type] = true;

            // 人物选择界面的宠物预览（静态展示用，不跑 AI）
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] =
                ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], frameSpeed)
                    .WithOffset(-10, -20f)     // 预览相对位置，可按需要微调
                    .WithSpriteDirection(-1)   // 朝向
                    .WithCode(DelegateMethods.CharacterPreview.Float); // 浮空效果
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.BlackCat); // 复制微风鱼的移动/碰撞等
            // AIType = ProjectileID.BlackCat;                  // 使用微风鱼 AI
            // 命中箱、透明度等均沿用 CloneDefaults，必要时再微调
            Projectile.width = 66;
            Projectile.height = 38;
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            player.petFlagFennecFox = false; // 避免与 AIType 的遗留标志冲突
            return true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 只要玩家活着且拥有 Buff，就让宠物存活
            if (!player.dead && player.HasBuff(ModContent.BuffType<KyubeyPetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            // 你也可以在这里加上简单的光效或尘埃效果
            // if (Main.rand.NextBool(90)) Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemDiamond);

            // --- 动画逻辑 ---
            if (Projectile.velocity.LengthSquared() > 0.5f)
            {
                // 只有在移动时才推进动画
                Projectile.frameCounter++;
                if (Projectile.frameCounter >= frameSpeed)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame >= Main.projFrames[Projectile.type])
                        Projectile.frame = 0;
                }
                runFlag = true;
            }
            else
            {
                // 静止时固定在第3帧（站立帧）
                Projectile.frame = 3;
                runFlag = false;
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
                Projectile.spriteDirection = Projectile.direction;
            }
        }

        // 可选：自定义帧速（不写则使用 ZephyrFish 的帧控制）
        // public override void FindFrame(int frameHeight)
        // {
        //     // 统一按 6 帧循环；每 6 tick 切换一帧
        //     Projectile.frameCounter++;
        //     if (Projectile.frameCounter >= 6)
        //     {
        //         Projectile.frameCounter = 0;
        //         Projectile.frame++;
        //         if (Projectile.frame >= Main.projFrames[Projectile.type])
        //         {
        //             Projectile.frame = 0;
        //         }
        //     }
        // }
    }
}