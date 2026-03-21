using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Active;
using WuDao.Content.Juexue.Passive;

namespace WuDao.Content.Global.NPCs
{
    [AutoloadHead]
    public class WanderingBeggar : ModNPC
    {
        private string shopName = Language.GetTextValue("Mods.WuDao.NPCs.WanderingBeggar.Shop.Name");
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
        // TODO: 测试流浪乞丐的方向
        // public override void PostAI()
        // {
        //     bool isTalking = Main.LocalPlayer.talkNPC == NPC.whoAmI;

        //     if (!Main.dayTime && !isTalking)
        //     {
        //         NPC.active = false;
        //         NPC.netUpdate = true;
        //         return;
        //     }

        //     NPC.spriteDirection = NPC.direction;
        // }
        public override void PostAI()
        {
            // 只在白天停留；夜晚由服务端/单机直接移除
            if (!Main.dayTime)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.active = false;
                    NPC.netUpdate = true;
                }
                return;
            }

            base.PostAI();
        }
        public override string GetChat()
        {
            return Language.GetTextValue("Mods.WuDao.NPCs.WanderingBeggar.Chat.Default");
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
                shopName = Language.GetTextValue(this.shopName);
            }
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, Language.GetTextValue(this.shopName));

            shop.Add<SharkWhaleFist>(new Condition(Language.GetTextValue("Conditions.DownedKingSlime"), () => NPC.downedSlimeKing));
            shop.Add<MagneticHeavenBlade>(new Condition(Language.GetTextValue("Conditions.DownedKingSlime"), () => NPC.downedSlimeKing));

            // 进度解锁（用 Condition 做谓词）
            shop.Add<QiankunShift>(new Condition(Language.GetTextValue("Conditions.DownedEyeOfCthulhu"), () => NPC.downedBoss1));
            shop.Add<XiangLong18>(new Condition(Language.GetTextValue("Conditions.DownedEyeOfCthulhu"), () => NPC.downedBoss1));

            shop.Add<DiamondSkin>(new Condition(Language.GetTextValue("Conditions.DownedSkeletron"), () => NPC.downedBoss3));
            shop.Add<Feixian>(new Condition(Language.GetTextValue("Conditions.DownedSkeletron"), () => NPC.downedBoss3));

            shop.Add<TenThousandSwords>(new Condition(Language.GetTextValue("Conditions.InHardmode"), () => Main.hardMode));
            shop.Add<ShengLongBa>(new Condition(Language.GetTextValue("Conditions.InHardmode"), () => Main.hardMode));

            shop.Add<HeavenlyPetals>(new Condition(Language.GetTextValue("Conditions.DownedMechBossAny"), () => NPC.downedMechBossAny));
            shop.Add<Kamehameha>(new Condition(Language.GetTextValue("Conditions.DownedMechBossAny"), () => NPC.downedMechBossAny));

            shop.Add<LingboWeibu>(new Condition(Language.GetTextValue("Conditions.DownedMechBossAll"), () => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3));
            shop.Add<Stampede>(new Condition(Language.GetTextValue("Conditions.DownedMechBossAll"), () => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3));

            shop.Add<WhiteBoneClaw>(new Condition(Language.GetTextValue("Conditions.DownedDukeFishron"), () => NPC.downedFishron));
            shop.Add<BladeWaltz>(new Condition(Language.GetTextValue("Conditions.DownedEmpressOfLight"), () => NPC.downedEmpressOfLight));

            shop.Register();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.WuDao.NPCs.WanderingBeggar.Info"))
            });
        }
    }
}
