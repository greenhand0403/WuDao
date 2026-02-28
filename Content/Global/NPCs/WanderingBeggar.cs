using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Active;
using WuDao.Content.Juexue.Passive;

namespace WuDao.Content.Global.NPCs
{
    // TODO: 含有中文提示信息
    // bug流浪乞丐不会在夜晚自动离开
    public class WanderingBeggar : ModNPC
    {
        public override void SetStaticDefaults()
        {
            // 26帧
            Main.npcFrameCount[Type] = 20;
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
            NPCID.Sets.CannotSitOnFurniture[Type] = true; // ✅ 避免坐下状态用到不存在的动画
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.TravellingMerchant);

            NPC.friendly = true;
            NPC.lifeMax = 400;
            NPC.defense = 15;
            NPC.knockBackResist = 0.5f;
            AnimationType = -1;
        }
        // —— 允许命中的“白名单” ——
        // 先放入经典案例：腐烂的鸡蛋（原版能砸到城镇NPC）
        private static readonly int[] AllowedProjTypes = new int[] {
            ProjectileID.RottenEgg,
            // 需要时在这里追加其它原版弹幕ID
        };

        public override bool? CanBeHitByItem(Player player, Item item)
        {
            // 默认拒绝近战/道具直接命中（有特殊原版道具再按需加白名单 ItemID）
            return false;
        }
        // 0-15 walk (16 frames)
        // 16-17 idle (2 frames)
        // 18-19 talk (2 frames)
        public override void FindFrame(int frameHeight)
        {
            const int WalkStart = 0;
            const int WalkCount = 16;

            const int IdleStart = 16;
            const int IdleCount = 2;

            const int TalkStart = 18;
            const int TalkCount = 2;

            // 交谈优先：玩家正在跟他对话时，强制播放交谈帧
            bool isTalking = Main.LocalPlayer.talkNPC == NPC.whoAmI;

            if (isTalking)
            {
                // 交谈动画速度（数字越小越快）
                NPC.frameCounter++;
                int talkFrame = TalkStart + (int)(NPC.frameCounter / 60) % TalkCount;
                NPC.frame.Y = talkFrame * frameHeight;
                return;
            }

            // 行走：有水平速度就播放行走帧
            if (System.Math.Abs(NPC.velocity.X) > 0.1f)
            {
                // 行走动画速度（数字越小越快）
                NPC.frameCounter++;
                int walkFrame = WalkStart + (int)(NPC.frameCounter / 3) % WalkCount;
                NPC.frame.Y = walkFrame * frameHeight;
                return;
            }

            // 待机：站着不动播放待机帧
            NPC.frameCounter++;
            int idleFrame = IdleStart + (int)(NPC.frameCounter / 60) % IdleCount;
            NPC.frame.Y = idleFrame * frameHeight;
        }
        // 第一层：仅允许白名单弹幕命中（玩家自有弹幕）
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.owner >= 0 && !projectile.npcProj && !projectile.hostile)
            {
                for (int i = 0; i < AllowedProjTypes.Length; i++)
                {
                    if (projectile.type == AllowedProjTypes[i])
                        return true;        // 放行白名单
                }
                return false;               // 拒绝其余玩家投射物（例如泰拉刃光束）
            }
            // 怪的/陷阱/环境等按原版处理（允许怪物伤到他）
            return null;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // 生成由 BeggarSystem / 包子控制
            return 0f;
        }

        public override void PostAI()
        {
            bool isTalking = Main.LocalPlayer.talkNPC == NPC.whoAmI;

            if (!Main.dayTime && !isTalking)
            {
                NPC.EncourageDespawn(10);
            }

            NPC.spriteDirection = NPC.direction;
        }

        public override string GetChat()
        {
            return "少侠请留步，我看你骨骼清奇，天人之资，必是练武奇才！不如买本武林绝学吧？";
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            // 左键按钮显示“商店”
            button = Language.GetTextValue("LegacyInterface.28"); // “Shop”
            button2 = ""; // 无第二按钮
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (firstButton)
            {
                // 打开商店（与 AddShops 注册的店铺关联）
                shopName = "绝学铺";
            }
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, "绝学铺");

            shop.Add<SharkWhaleFist>(new Condition("击败史莱姆王", () => NPC.downedSlimeKing));
            shop.Add<MagneticHeavenBlade>(new Condition("击败史莱姆王", () => NPC.downedSlimeKing));

            // 进度解锁（用 Condition 做谓词）
            shop.Add<QiankunShift>(new Condition("击败克苏鲁之眼", () => NPC.downedBoss1));
            shop.Add<XiangLong18>(new Condition("击败克苏鲁之眼", () => NPC.downedBoss1));

            shop.Add<DiamondSkin>(new Condition("击败骷髅王", () => NPC.downedBoss3));
            shop.Add<Feixian>(new Condition("击败骷髅王", () => NPC.downedBoss3));

            shop.Add<TenThousandSwords>(new Condition("进入困难模式", () => Main.hardMode));
            shop.Add<ShengLongBa>(new Condition("进入困难模式", () => Main.hardMode));

            shop.Add<HeavenlyPetals>(new Condition("击败任意机械BOSS", () => NPC.downedMechBossAny));
            shop.Add<Kamehameha>(new Condition("击败任意机械BOSS", () => NPC.downedMechBossAny));

            shop.Add<LingboWeibu>(new Condition("击败全部机械Boss", () => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3));
            shop.Add<Stampede>(new Condition("击败全部机械Boss", () => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3));

            shop.Add<WhiteBoneClaw>(new Condition("击败猪鲨", () => NPC.downedFishron));
            shop.Add<BladeWaltz>(new Condition("击败光女", () => NPC.downedHalloweenKing));

            shop.Register();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("行遍四方的流浪者，据说掌握江湖奇书的门路。会被馒头吸引，夜晚时自动离去。")
            });
        }
    }
}
