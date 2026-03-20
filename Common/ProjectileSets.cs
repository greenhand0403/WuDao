using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
// 随机取一支箭
// int arrowRandom = ProjectileSets.ArrowSet.Get(SelectionMode.Random);
// 按顺序取第 3 个箭
// int arrowOrdered = ProjectileSets.ArrowSet.Get(SelectionMode.Ordered, 3);
// 循环取第 15 个箭（超出长度会循环回去）
// int arrowLoop = ProjectileSets.ArrowSet.Get(SelectionMode.Loop, 15);
// 原版物品、射弹的重新归类集合
namespace WuDao.Common
{
    public enum SelectionMode
    {
        Random,
        Ordered,
        Loop
    }
    public static class HashSetExtensionsUnified
    {
        /// <summary>
        /// 根据模式从 HashSet 中取元素
        /// </summary>
        /// <param name="set">目标集合</param>
        /// <param name="index">索引（仅 Ordered 和 Loop 模式下有效）</param>
        /// <param name="mode">选择模式</param>
        public static int Get(this HashSet<int> set, SelectionMode mode, int index = 0)
        {
            if (set == null || set.Count == 0)
                throw new InvalidOperationException("hashset is null or empty");

            switch (mode)
            {
                case SelectionMode.Random:
                    return set.ElementAt(Main.rand.Next(set.Count));

                case SelectionMode.Ordered:
                    if (index < 0 || index >= set.Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return set.OrderBy(x => x).ElementAt(index);

                case SelectionMode.Loop:
                    var ordered = set.OrderBy(x => x).ToList();
                    return ordered[index % ordered.Count];

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), "invalid selection mode");
            }
        }
    }
    public static class ItemSets
    {
        /// <summary>
        /// 五行盔甲覆盖范围：铜/锡/铁/铅/银/钨/金/铂金/陨石/钴/钯/秘银/山铜(Orichalcum)/精金/泰坦/叶绿（含多种头盔分支）
        /// </summary>
        public static readonly HashSet<int> OreArmorSet = new()
        {
            // —— 预困难 ores —— //
            // 铜 Copper
            ItemID.CopperHelmet, ItemID.CopperChainmail, ItemID.CopperGreaves,
            // 锡 Tin
            ItemID.TinHelmet, ItemID.TinChainmail, ItemID.TinGreaves,
            // 铁 Iron
            ItemID.IronHelmet, ItemID.IronChainmail, ItemID.IronGreaves,
            // 铅 Lead
            ItemID.LeadHelmet, ItemID.LeadChainmail, ItemID.LeadGreaves,
            // 银 Silver
            ItemID.SilverHelmet, ItemID.SilverChainmail, ItemID.SilverGreaves,
            // 钨 Tungsten
            ItemID.TungstenHelmet, ItemID.TungstenChainmail, ItemID.TungstenGreaves,
            // 金 Gold
            ItemID.GoldHelmet, ItemID.GoldChainmail, ItemID.GoldGreaves,
            // 铂金 Platinum
            ItemID.PlatinumHelmet, ItemID.PlatinumChainmail, ItemID.PlatinumGreaves,

            // 陨石 Meteorite（原版常视作“矿物系”进度）
            ItemID.MeteorHelmet, ItemID.MeteorSuit, ItemID.MeteorLeggings,

            // —— 困难 ores —— //
            // Cobalt（头：Helmet/Mask/Headgear）
            ItemID.CobaltHelmet, ItemID.CobaltMask, ItemID.CobaltHat,
            ItemID.CobaltBreastplate, ItemID.CobaltLeggings,

            // Palladium（头：Helmet/Mask/Headgear）
            ItemID.PalladiumHelmet, ItemID.PalladiumMask, ItemID.PalladiumHeadgear,
            ItemID.PalladiumBreastplate, ItemID.PalladiumLeggings,

            // Mythril（头：Helmet/Mask/Hood）
            ItemID.MythrilHelmet, ItemID.MythrilHat, ItemID.MythrilHood,
            ItemID.MythrilChainmail, ItemID.MythrilGreaves,

            // Orichalcum（头：Helmet/Mask/Headgear）
            ItemID.OrichalcumHelmet, ItemID.OrichalcumMask, ItemID.OrichalcumHeadgear,
            ItemID.OrichalcumBreastplate, ItemID.OrichalcumLeggings,

            // Adamantite（头：Helmet/Mask/Headgear）
            ItemID.AdamantiteHelmet, ItemID.AdamantiteMask, ItemID.AdamantiteHeadgear,
            ItemID.AdamantiteBreastplate, ItemID.AdamantiteLeggings,

            // Titanium（头：Helmet/Mask/Headgear）
            ItemID.TitaniumHelmet, ItemID.TitaniumMask, ItemID.TitaniumHeadgear,
            ItemID.TitaniumBreastplate, ItemID.TitaniumLeggings,

            // —— 丛林后期矿：叶绿 Chlorophyte（头：Helmet/Mask/Headgear） —— //
            ItemID.ChlorophyteHelmet, ItemID.ChlorophyteMask, ItemID.ChlorophyteHeadgear,
            ItemID.ChlorophytePlateMail, ItemID.ChlorophyteGreaves,
        };
        // ?默认启用的刀光染色物品（包含所有刀光颜色）
        public static readonly HashSet<int> BladeTrailSet = new()
        {
            ItemID.PalmWoodSword,// 珍珠木剑
            ItemID.WoodenSword,// 木剑
            ItemID.BorealWoodSword,// 枯木剑
            ItemID.CopperBroadsword,// 铜阔剑
            ItemID.RichMahoganySword,// 红木剑
            ItemID.CactusSword,// 仙人掌剑
            ItemID.TinBroadsword,// 锡阔剑
            ItemID.LeadBroadsword,// 铅阔剑
            ItemID.EbonwoodSword,// 灰木剑
            ItemID.ShadewoodSword,// 阴影木剑
            ItemID.IronBroadsword,// 铁阔剑
            ItemID.SilverBroadsword,// 银阔剑
            ItemID.GoldBroadsword,// 金阔剑
            ItemID.PlatinumBroadsword,// 铂金阔剑
            ItemID.ZombieArm,// 僵尸臂
            ItemID.TungstenBroadsword,// 钨阔剑
            ItemID.AshWoodSword,// 灰木剑
            ItemID.Flymeal,// 臭虫剑
            ItemID.AntlionClaw,// 蚁狮爪
            ItemID.BoneSword,// 骨剑
            ItemID.BatBat,// 蝙蝠蝙蝠
            ItemID.TentacleSpike,// 触手尖刺
            ItemID.CandyCaneSword,// 糖果剑
            ItemID.Katana,// 武士刀
            ItemID.IceBlade,// 冰刃
            ItemID.LightsBane,// 光刃
            ItemID.Muramasa,// 村正
            ItemID.BloodButcherer,// 血腥屠刀
            ItemID.Starfury,// 星怒
            ItemID.EnchantedSword,// 附魔剑
            ItemID.PurpleClubberfish,// 紫挥棒鱼
            ItemID.BeeKeeper,// 养蜂人
            ItemID.FalconBlade,// 猎鹰刃
            ItemID.BladeofGrass,// 草刃
            ItemID.NightsEdge,// 永夜
            ItemID.PearlwoodSword,// 珍珠木剑
            ItemID.DyeTradersScimitar,// 异域弯刀
            ItemID.TaxCollectorsStickOfDoom,// 精致手杖
            ItemID.SlapHand,//  拍拍手
            ItemID.CobaltSword,// 钴剑
            ItemID.PalladiumSword,// 钯金剑
            ItemID.MythrilSword,// 秘银剑
            ItemID.OrichalcumSword,// 山铜剑
            ItemID.BreakerBlade,// 毁灭刃
            ItemID.Cutlass,// 短弯刀
            ItemID.Frostbrand,// 霜刃
            ItemID.AdamantiteSword,// 精金剑
            ItemID.BeamSword,// 光束剑
            ItemID.TitaniumSword,// 钛金剑
            ItemID.Bladetongue,// 舌锋剑
            ItemID.HamBat,// 火腿棍
            // ItemID.Excalibur,
            ItemID.WaffleIron,// 华夫饼烤模
            // ItemID.TrueNightsEdge,
            // ItemID.TrueExcalibur,
            ItemID.PsychoKnife,// 变态人的刀
            ItemID.Keybrand,// 钥匙剑
            ItemID.ChlorophyteClaymore,// 叶绿双刃刀
            // ItemID.TheHorsemansBlade,
            ItemID.DD2SquireDemonSword,// 地狱之剑
            ItemID.DD2SquireBetsySword,// 飞龙
            ItemID.StarWrath,// 狂星之怒
            ItemID.InfluxWaver,// 波涌剑
            // ItemID.TerraBlade,
            ItemID.Meowmere,// 彩虹猫之刃
            ItemID.ChlorophyteSaber,// 叶绿军刀
            ItemID.ChristmasTreeSword,// 圣诞树剑
            ItemID.Seedler,// 种子弯刀
        };
        // 白名单
        public static readonly HashSet<int> YuJianWhitelistSet = new()
        {
            // 白名单包含部分魔法剑
            ItemID.SkyFracture,// 裂天剑
            ItemID.Zenith,// 天顶剑
            ItemID.Excalibur,// 断钢剑
            ItemID.TrueNightsEdge,// 真永夜剑
            ItemID.TrueExcalibur,// 真断钢剑
            ItemID.TheHorsemansBlade,// 无头骑士之剑
            ItemID.TerraBlade,// 泰拉刃
            ItemID.BrokenHeroSword, // 断裂英雄剑
            ItemID.Swordfish, // 剑鱼
            ItemID.ObsidianSwordfish, // 黑曜石剑鱼
            ItemID.SwordWhip, // 迪朗达尔
            ItemID.PiercingStarlight, // 星光
            ItemID.EmpressBlade, // 泰拉棱镜
        };
        /// <summary>
        /// 判断物品是否为可御剑武器(白名单包含部分魔法剑和特殊剑形物品 + 默认刀光染色剑集合 - 部分角度不对的剑已排除)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsYuJianSword(Item item) => item != null && !item.IsAir && (YuJianWhitelistSet.Contains(item.type) || BladeTrailSet.Contains(item.type));
        // 万剑归宗和无敌包含的剑型射弹
        public static readonly HashSet<int> SwordItemSet = new()
        {
            ItemID.CopperBroadsword,// 铜阔剑
            ItemID.LightsBane,// 魔光剑
            ItemID.Muramasa,// 村正
            ItemID.Terragrim,// 泰拉魔刃
            ItemID.BloodButcherer,// 血腥屠刀
            ItemID.Starfury,// 星怒
            ItemID.EnchantedSword,// 附魔剑
            ItemID.BeeKeeper,// 养蜂人
            ItemID.BladeofGrass,// 草刃
            ItemID.FieryGreatsword,// 火山
            ItemID.NightsEdge,// 永夜
            ItemID.TrueNightsEdge,// 真永夜剑
            ItemID.TrueExcalibur,// 真断钢剑
            ItemID.Excalibur,// 断钢剑
            ItemID.Seedler,// 种子弯刀
            ItemID.TerraBlade,// 泰拉刃
            ItemID.TheHorsemansBlade,// 无头骑士之剑
            ItemID.StarWrath,// 狂星之怒
            ItemID.Meowmere,// 彩虹猫之刃
            ItemID.InfluxWaver,// 波涌剑
            ItemID.Zenith,// 天顶剑
        };
        // 所有的光剑
        public static readonly HashSet<int> PhasebladeSet = new()
        {
            ItemID.RedPhaseblade,// 红陨石光剑
            ItemID.BluePhaseblade,// 蓝陨石光剑
            ItemID.GreenPhaseblade,// 绿陨石光剑
            ItemID.YellowPhaseblade,// 黄陨石光剑
            ItemID.PurplePhaseblade,// 紫陨石光剑
            ItemID.OrangePhaseblade,// 橙陨石光剑
            ItemID.WhitePhaseblade,// 白陨石光剑
            ItemID.BluePhasesaber,// 蓝晶光刃
            ItemID.PurplePhasesaber,// 紫晶光刃
            ItemID.OrangePhasesaber,// 橙晶光刃
            ItemID.WhitePhasesaber,// 白晶光刃
        };
        // 所有的宝石
        public static readonly HashSet<int> GemSet = new()
        {
            ItemID.Diamond,// 钻石
            ItemID.Amber,// 琥珀
            ItemID.Emerald,// 翡翠
            ItemID.Ruby,// 红玉
            ItemID.Sapphire,// 蓝玉
            ItemID.Topaz,// 钻石
            ItemID.Amethyst,// 紫晶
        };
        // 所有的水果
        public static readonly HashSet<int> FruitSet = new()
        {
            ItemID.Apple,// 苹果
            ItemID.Apricot,// 杏
            ItemID.Grapefruit,// 葡萄柚
            ItemID.Lemon,// 柠檬
            ItemID.Peach,// 桃子
            ItemID.Cherry,// 樱桃
            ItemID.Plum,// 李子
            ItemID.BlackCurrant,// 黑莓
            ItemID.Elderberry,// 老番茄
            ItemID.BloodOrange,// 血橙
            ItemID.Rambutan,// 拉姆顿
            ItemID.Mango,// 芒果
            ItemID.Pineapple,// 菠萝
            ItemID.Banana,// 香蕉
            ItemID.Coconut,// 椰子
            ItemID.Dragonfruit,//  дра果
            ItemID.Starfruit,// 星果
            ItemID.Pomegranate,// 石榴
            ItemID.SpicyPepper,// 辣椒
        };
        // 所有的蛋
        public static readonly HashSet<int> EggSet = new()
        {
            ItemID.DD2BetsyPetItem,// 贝蒂宠物蛋
            ItemID.BlueEgg,// 蓝色蛋
            ItemID.RottenEgg,// 腐烂蛋
            ItemID.DD2PetGato,// 猫宠物蛋
            ItemID.LizardEgg,// 蜥蜴蛋
            ItemID.SpiderEgg,// 蜘蛛蛋
            ItemID.FriedEgg,// 煎蛋
            ItemID.DD2PetGhost,// 幽灵宠物蛋
        };
        // 所有任务鱼
        public static readonly HashSet<int> TaskFishSet = new()
        {
            2475, 2476, 2450, 2477, 2478, 2451, 2479, 2480, 2452, 2453, 2481, 2454, 2482, 2483, 2455, 2456, 2457, 2458, 2459, 2460, 2484, 2472, 2461, 2462, 2463, 2485, 2464, 2465, 2486, 2466, 2467, 2468, 2487, 2469, 2488, 2470, 2471, 2473, 2474, 4393, 4394
        };
    }
    public static class ProjectileSets
    {
        /// <summary>所有的箭，注意箭尖的方向！</summary>
        public static readonly HashSet<int> ArrowSet = new()
        {
            // 朝下
            ProjectileID.WoodenArrowFriendly,// 木剑
            ProjectileID.FireArrow,// 火箭
            ProjectileID.UnholyArrow,// 邪恶箭
            ProjectileID.JestersArrow,// 小丑箭
            ProjectileID.HellfireArrow,// 地狱火箭
            ProjectileID.HolyArrow,// 神圣箭
            ProjectileID.CursedArrow,// 诅咒箭
            ProjectileID.FrostburnArrow,// 冰霜箭
            ProjectileID.ChlorophyteArrow,// 叶绿箭
            ProjectileID.IchorArrow,// 灵液箭
            ProjectileID.VenomArrow,// VenomArrow
            ProjectileID.BeeArrow,// 蜂箭
            ProjectileID.BoneArrowFromMerchant,// 从商人处获得的骨箭
            ProjectileID.PhantasmArrow,// 幻影箭
            ProjectileID.MoonlordArrow,// 月亮领主箭
            // 朝上
            ProjectileID.BoneArrow,// 骨箭
            ProjectileID.FrostArrow,// 冰霜箭
            ProjectileID.ShadowFlameArrow,// 暗影箭
            ProjectileID.DD2BetsyArrow,// 贝蒂宠物箭
            ProjectileID.BloodArrow,// 血箭
            // 朝右
            ProjectileID.ShimmerArrow,// 微光箭
            ProjectileID.Hellwing// 狱火箭
        };
        /// <summary>所有的照明弹</summary>
        public static readonly HashSet<int> FlareSet = new()
        {
            ProjectileID.Flare,// 火把
            ProjectileID.BlueFlare,// 蓝色火把
            ProjectileID.SpelunkerFlare,//  spelunker火把
            ProjectileID.CursedFlare,// 诅咒火把
            ProjectileID.RainbowFlare,// 彩虹火把
            ProjectileID.ShimmerFlare// 微光火把
        };
        /// <summary>所有的子弹</summary>
        public static readonly HashSet<int> BulletSet = new()
        {
            // 向右
            ProjectileID.Bullet,// 子弹
            ProjectileID.GreenLaser,// 绿色激光
            ProjectileID.MeteorShot,// 流星弹
            ProjectileID.CrystalBullet,// 水晶弹
            ProjectileID.CursedBullet,// 诅咒弹
            ProjectileID.ChlorophyteBullet,// 叶绿弹
            ProjectileID.BulletHighVelocity,// 高速度弹
            ProjectileID.IchorBullet,// 灵液弹
            ProjectileID.VenomBullet,// VenomBullet
            ProjectileID.PartyBullet,// 派对弹
            ProjectileID.NanoBullet,// 纳米弹
            ProjectileID.ExplosiveBullet,// 爆炸弹
            ProjectileID.GoldenBullet,// 黄金弹
            ProjectileID.MoonlordBullet,// 月亮领主弹
            ProjectileID.SilverBullet,// 银弹
        };
        /// <summary>
        /// 所有的回旋镖
        /// </summary>
        public static readonly HashSet<int> BoomerangSet = new()
        {
            // 向左
            ProjectileID.EnchantedBoomerang,// 附魔回旋镖
            ProjectileID.Flamarang,//  Flamarang
            ProjectileID.ThornChakram,// 荆棘 Chakram
            ProjectileID.WoodenBoomerang,// 木剑回旋镖
            ProjectileID.LightDisc,// 光弹
            ProjectileID.IceBoomerang,// 冰霜回旋镖
            ProjectileID.PossessedHatchet,// 被Possess的Hatchet
            ProjectileID.Bananarang,// 香蕉回旋镖
            ProjectileID.PaladinsHammerFriendly,// 圣骑士的锤子
            ProjectileID.BloodyMachete,// 血腥锤
            ProjectileID.FruitcakeChakram,// 水果蛋糕Chakram
            ProjectileID.Shroomerang,// 蘑菇回旋镖
            ProjectileID.BouncingShield,// 弹跳盾牌
            ProjectileID.Trimarang,// 三向回旋镖
            ProjectileID.CombatWrench,// 战斗锤
        };
        /// <summary>所有的镰刀</summary>
        public static readonly HashSet<int> ScytheSet = new()
        {
            ProjectileID.DemonScythe,// 恶魔镰刀
            ProjectileID.IceSickle,// 冰霜镰刀
            ProjectileID.DeathSickle,// 死亡镰刀
            ProjectileID.Electrosphere,// 电子球体
        };
        /// <summary>所有的飞刀</summary>
        public static readonly HashSet<int> KnifeSet = new()
        {
            ProjectileID.ThrowingKnife,// 投刀
            ProjectileID.PoisonedKnife,// 中毒飞刀
            ProjectileID.MagicDagger,// 魔法飞刀
            ProjectileID.VampireKnife,// 吸血鬼飞刀
            ProjectileID.StarAnise,// 星型茴香
            ProjectileID.ShadowFlameKnife,// 暗影飞刀
            ProjectileID.FrostDaggerfish// 寒霜飞鱼
        };
        /// <summary>所有的激光</summary>
        public static readonly HashSet<int> LaserSet = new()
        {
            ProjectileID.PurpleLaser,// 紫色激光
            ProjectileID.RainFriendly,// 雨水激光
            ProjectileID.BloodRain,// 血雨激光
            ProjectileID.HeatRay,// 加热射线
            ProjectileID.LostSoulFriendly,// 丢失的灵魂激光
            ProjectileID.InfernoFriendlyBolt,// 火激光
            ProjectileID.ShadowBeamFriendly,// 暗影光束
            ProjectileID.MiniRetinaLaser,// 迷你视网膜激光
            ProjectileID.ChargedBlasterLaser,// 充能激光
            ProjectileID.MinecartMechLaser,// 矿车机械激光
            ProjectileID.ScutlixLaser,// Scutlix激光
            ProjectileID.MoonlordTurretLaser// 月亮领主炮塔激光
        };
    }
    // UN USE
    public static class NPCSets
    {
        // 变形怪
        public static readonly HashSet<int> MorphingSet = new()
        {
            NPCID.Harpy,// 鸟妖
            NPCID.Nymph,// 女妖
            NPCID.DesertLamiaLight,// 沙漠 Lama 亮
            NPCID.DesertLamiaDark,// 沙漠 Lama 暗
            NPCID.Medusa,// 美杜莎
            NPCID.TheBride,// 新娘
            NPCID.PirateCrossbower// 海盗交叉棒
        };
        // 蝙蝠
        public static readonly HashSet<int> BatSet = new()
        {
            NPCID.CaveBat,// 洞穴蝙蝠
            NPCID.Hellbat,// 地狱蝙蝠
            NPCID.IceBat,// 冰霜蝙蝠
            NPCID.JungleBat,// 丛林蝙蝠
            NPCID.SporeBat,// 孢子蝙蝠
            NPCID.GiantBat,// 巨人蝙蝠
            NPCID.IlluminantBat,// 夜明蝙蝠
            NPCID.Lavabat// 熔岩蝙蝠
        };
    }
}
