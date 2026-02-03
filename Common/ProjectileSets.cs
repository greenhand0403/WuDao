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
                throw new InvalidOperationException("集合为空");

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
                    throw new ArgumentOutOfRangeException(nameof(mode), "不支持的选择模式");
            }
        }
    }
    public static class ItemSets
    {
        /// <summary>
        /// 覆盖范围：铜/锡/铁/铅/银/钨/金/铂金/陨石/钴/钯/秘银/山铜(Orichalcum)/精金/泰坦/叶绿（含多种头盔分支）
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
        public static readonly HashSet<int> BladeTrailSet = new()
        {
            ItemID.CopperBroadsword,
            ItemID.PalmWoodSword,
            ItemID.WoodenSword,
            ItemID.BorealWoodSword,
            ItemID.CopperBroadsword,
            ItemID.RichMahoganySword,
            ItemID.CactusSword,
            ItemID.TinBroadsword,
            ItemID.LeadBroadsword,
            ItemID.EbonwoodSword,
            ItemID.ShadewoodSword,
            ItemID.IronBroadsword,
            ItemID.SilverBroadsword,
            ItemID.GoldBroadsword,
            ItemID.PlatinumBroadsword,
            ItemID.ZombieArm,
            ItemID.TungstenBroadsword,
            ItemID.AshWoodSword,
            ItemID.Flymeal,
            ItemID.AntlionClaw,
            ItemID.BoneSword,
            ItemID.BatBat,
            ItemID.TentacleSpike,
            ItemID.CandyCaneSword,
            ItemID.Katana,
            ItemID.IceBlade,
            ItemID.LightsBane,
            ItemID.Muramasa,
            ItemID.BloodButcherer,
            ItemID.Starfury,
            ItemID.EnchantedSword,
            ItemID.PurpleClubberfish,
            ItemID.BeeKeeper,
            ItemID.FalconBlade,
            ItemID.BladeofGrass,
            ItemID.NightsEdge,
            ItemID.PearlwoodSword,
            ItemID.DyeTradersScimitar,
            ItemID.TaxCollectorsStickOfDoom,
            ItemID.SlapHand,
            ItemID.CobaltSword,
            ItemID.PalladiumSword,
            ItemID.MythrilSword,
            ItemID.OrichalcumSword,
            ItemID.BreakerBlade,
            ItemID.Cutlass,
            ItemID.Frostbrand,
            ItemID.AdamantiteSword,
            ItemID.BeamSword,
            ItemID.TitaniumSword,
            ItemID.Bladetongue,
            ItemID.HamBat,
            // ItemID.Excalibur,
            ItemID.WaffleIron,
            // ItemID.TrueNightsEdge,
            // ItemID.TrueExcalibur,
            ItemID.PsychoKnife,
            ItemID.Keybrand,
            ItemID.ChlorophyteClaymore,
            // ItemID.TheHorsemansBlade,
            ItemID.DD2SquireDemonSword,
            ItemID.DD2SquireBetsySword,
            ItemID.StarWrath,
            ItemID.InfluxWaver,
            // ItemID.TerraBlade,
            ItemID.Meowmere,
            ItemID.ChlorophyteSaber,
            ItemID.ChristmasTreeSword,
            ItemID.Seedler
        };
        public static readonly HashSet<int> SwordItemSet = new()
        {
            ItemID.CopperBroadsword,
            ItemID.LightsBane,
            ItemID.Muramasa,
            ItemID.Terragrim,
            ItemID.BloodButcherer,
            ItemID.Starfury,
            ItemID.EnchantedSword,
            ItemID.BeeKeeper,
            ItemID.BladeofGrass,
            ItemID.FieryGreatsword,
            ItemID.NightsEdge,
            ItemID.TrueNightsEdge,
            ItemID.TrueExcalibur,
            ItemID.Excalibur,
            ItemID.Seedler,
            ItemID.TerraBlade,
            ItemID.TheHorsemansBlade,
            ItemID.StarWrath,
            ItemID.Meowmere,
            ItemID.InfluxWaver,
            ItemID.Zenith
        };
        public static readonly HashSet<int> PhasebladeSet = new()
        {
            ItemID.RedPhaseblade,
            ItemID.BluePhaseblade,
            ItemID.GreenPhaseblade,
            ItemID.YellowPhaseblade,
            ItemID.PurplePhaseblade,
            ItemID.OrangePhaseblade,
            ItemID.WhitePhaseblade,
            ItemID.BluePhasesaber,
            ItemID.PurplePhasesaber,
            ItemID.OrangePhasesaber,
            ItemID.WhitePhasesaber,
            ItemID.PurplePhasesaber,
            ItemID.OrangePhasesaber,
            ItemID.WhitePhasesaber,
        };
        public static readonly HashSet<int> GemSet = new()
        {
            ItemID.Diamond,
            ItemID.Amber,
            ItemID.Emerald,
            ItemID.Ruby,
            ItemID.Sapphire,
            ItemID.Topaz,
            ItemID.Amethyst
        };
        public static readonly HashSet<int> FruitSet = new()
        {
            ItemID.Apple,
            ItemID.Apricot,
            ItemID.Grapefruit,
            ItemID.Lemon,
            ItemID.Peach,
            ItemID.Cherry,
            ItemID.Plum,
            ItemID.BlackCurrant,
            ItemID.Elderberry,
            ItemID.BloodOrange,
            ItemID.Rambutan,
            ItemID.Mango,
            ItemID.Pineapple,
            ItemID.Banana,
            ItemID.Coconut,
            ItemID.Dragonfruit,
            ItemID.Starfruit,
            ItemID.Pomegranate,
            ItemID.SpicyPepper
        };
        public static readonly HashSet<int> EggSet = new()
        {
            ItemID.DD2BetsyPetItem,
            ItemID.BlueEgg,
            ItemID.RottenEgg,
            ItemID.DD2PetGato,
            ItemID.LizardEgg,
            ItemID.SpiderEgg,
            ItemID.FriedEgg,
            ItemID.DD2PetGhost
        };
    }
    public static class ProjectileSets
    {
        public static readonly HashSet<int> ArrowSet = new()
        {
            // 朝下
            ProjectileID.WoodenArrowFriendly,
            ProjectileID.FireArrow,
            ProjectileID.UnholyArrow,
            ProjectileID.JestersArrow,
            ProjectileID.HellfireArrow,
            ProjectileID.HolyArrow,
            ProjectileID.CursedArrow,
            ProjectileID.FrostburnArrow,
            ProjectileID.ChlorophyteArrow,
            ProjectileID.IchorArrow,
            ProjectileID.VenomArrow,
            ProjectileID.BeeArrow,
            ProjectileID.BoneArrowFromMerchant,
            ProjectileID.PhantasmArrow,
            ProjectileID.MoonlordArrow,
            // 朝上
            ProjectileID.BoneArrow,
            ProjectileID.FrostArrow,
            ProjectileID.ShadowFlameArrow,
            ProjectileID.DD2BetsyArrow,
            ProjectileID.BloodArrow,
            // 朝右
            ProjectileID.ShimmerArrow,
            ProjectileID.Hellwing
        };

        public static readonly HashSet<int> FlareSet = new()
        {
            ProjectileID.Flare,
            ProjectileID.BlueFlare,
            ProjectileID.SpelunkerFlare,
            ProjectileID.CursedFlare,
            ProjectileID.RainbowFlare,
            ProjectileID.ShimmerFlare
        };

        public static readonly HashSet<int> BulletSet = new()
        {
            // 向右
            ProjectileID.Bullet,
            ProjectileID.GreenLaser,
            ProjectileID.MeteorShot,
            ProjectileID.CrystalBullet,
            ProjectileID.CursedBullet,
            ProjectileID.ChlorophyteBullet,
            ProjectileID.BulletHighVelocity,
            ProjectileID.IchorBullet,
            ProjectileID.VenomBullet,
            ProjectileID.PartyBullet,
            ProjectileID.NanoBullet,
            ProjectileID.ExplosiveBullet,
            ProjectileID.GoldenBullet,
            ProjectileID.MoonlordBullet,
            ProjectileID.SilverBullet
        };

        public static readonly HashSet<int> BoomerangSet = new()
        {
            // 向左
            ProjectileID.EnchantedBoomerang,
            ProjectileID.Flamarang,
            ProjectileID.ThornChakram,
            ProjectileID.WoodenBoomerang,
            ProjectileID.LightDisc,
            ProjectileID.IceBoomerang,
            ProjectileID.PossessedHatchet,
            ProjectileID.Bananarang,
            ProjectileID.PaladinsHammerFriendly,
            ProjectileID.BloodyMachete,
            ProjectileID.FruitcakeChakram,
            ProjectileID.Shroomerang,
            ProjectileID.BouncingShield,
            ProjectileID.Trimarang,
            ProjectileID.CombatWrench
        };

        public static readonly HashSet<int> ScytheSet = new()
        {
            ProjectileID.DemonScythe,
            ProjectileID.IceSickle,
            ProjectileID.DeathSickle,
            ProjectileID.Electrosphere
        };

        public static readonly HashSet<int> KnifeSet = new()
        {
            ProjectileID.ThrowingKnife,
            ProjectileID.PoisonedKnife,
            ProjectileID.MagicDagger,
            ProjectileID.VampireKnife,
            ProjectileID.StarAnise,
            ProjectileID.ShadowFlameKnife,
            ProjectileID.FrostDaggerfish
        };
        public static readonly HashSet<int> LaserSet = new()
        {
            ProjectileID.PurpleLaser,
            ProjectileID.RainFriendly,
            ProjectileID.BloodRain,
            ProjectileID.HeatRay,
            ProjectileID.LostSoulFriendly,
            ProjectileID.InfernoFriendlyBolt,
            ProjectileID.ShadowBeamFriendly,
            ProjectileID.MiniRetinaLaser,
            ProjectileID.ChargedBlasterLaser,
            ProjectileID.MinecartMechLaser,
            ProjectileID.ScutlixLaser,
            ProjectileID.MoonlordTurretLaser
        };
    }
    public static class NPCSets
    {
        // 变形怪
        public static readonly HashSet<int> MorphingSet = new()
        {
            NPCID.Harpy,
            NPCID.Nymph,
            NPCID.DesertLamiaLight,
            NPCID.DesertLamiaDark,
            NPCID.Medusa,
            NPCID.TheBride,
            NPCID.PirateCrossbower
        };
        // 蝙蝠
        public static readonly HashSet<int> BatSet = new()
        {
            NPCID.CaveBat,
            NPCID.Hellbat,
            NPCID.IceBat,
            NPCID.JungleBat,
            NPCID.SporeBat,
            NPCID.GiantBat,
            NPCID.IlluminantBat,
            NPCID.Lavabat
        };
    }
}
