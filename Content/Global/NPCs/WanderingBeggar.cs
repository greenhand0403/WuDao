using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Active;
using WuDao.Content.Juexue.Passive;

namespace WuDao.Content.Global.NPCs
{
    // TODO: 增加流浪乞丐的贴图
    // 跟乞丐对话时，bug 绝学栏位置会发生偏移
    public class WanderingBeggar : ModNPC
    {
        public override string Texture => "Terraria/Images/NPC_368";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.TravellingMerchant];
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.TravellingMerchant);
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.lifeMax = 400;
            NPC.defense = 15;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.TravellingMerchant;
            NPC.dontTakeDamage = true;
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

        public override void AI()
        {
            // 夜晚离开
            if (!Main.dayTime)
            {
                NPC.EncourageDespawn(10);
            }
            NPC.spriteDirection = NPC.direction;   // 如发现还是反了，就改为 = -NPC.direction
        }

        public override string GetChat()
        {
            return "客官行行好……不如买本绝学吧？";
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

        // ✅ 改用 AddShops
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
            shop.Add<Stampede>(new Condition("击败全部机械Boss", () => NPC.downedMechBoss1&&NPC.downedMechBoss2&&NPC.downedMechBoss3));

            shop.Add<WhiteBoneClaw>(new Condition("击败猪鲨", () => NPC.downedFishron));
            shop.Add<BladeWaltz>(new Condition("击败光女", () => NPC.downedHalloweenKing));
            
            shop.Register();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("行遍四方的流浪者，据说掌握江湖奇书的门路。白天现身，夜晚离去。")
            });
        }
    }
}
