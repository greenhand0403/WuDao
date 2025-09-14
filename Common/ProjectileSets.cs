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
        public static readonly HashSet<int> BladeTrailSet = new()
        {
            ItemID.CopperBroadsword,
            ItemID.IronBroadsword,
            ItemID.SilverBroadsword,
            ItemID.GoldBroadsword,
            ItemID.PlatinumBroadsword,
            ItemID.Seedler
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
