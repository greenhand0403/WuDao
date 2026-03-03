using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace WuDao.Common
{
    public class BladeTrailConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        // ✅ 配置标题（显示在模组配置列表里）
        public override LocalizedText DisplayName =>
            Language.GetText("Mods.WuDao.Configs.BladeTrailConfig.DisplayName");
        // ✅ 全局刀光配置
        [Header("$Mods.WuDao.Configs.BladeTrailConfig.Headers.GlobalBladeTrail")]
        [LabelKey("$Mods.WuDao.Configs.BladeTrailConfig.EnableVertexBladeTrail.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailConfig.EnableVertexBladeTrail.Tooltip")]
        [DefaultValue(true)]
        public bool EnableVertexBladeTrail { get; set; } = true;
        // ✅ 默认白名单配置
        [Header("$Mods.WuDao.Configs.BladeTrailConfig.Headers.DefaultSets")]
        [LabelKey("$Mods.WuDao.Configs.BladeTrailConfig.IncludeDefaultBladeTrailSet.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailConfig.IncludeDefaultBladeTrailSet.Tooltip")]
        [DefaultValue(true)]
        public bool IncludeDefaultBladeTrailSet { get; set; } = true;
        // ✅ 包含光剑
        [LabelKey("$Mods.WuDao.Configs.BladeTrailConfig.IncludePhaseblades.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailConfig.IncludePhaseblades.Tooltip")]
        [DefaultValue(true)]
        public bool IncludePhaseblades { get; set; } = true;
        // ✅ 白名单配置
        [Header("$Mods.WuDao.Configs.BladeTrailConfig.Headers.CustomWhitelist")]
        [LabelKey("$Mods.WuDao.Configs.BladeTrailConfig.Whitelist.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailConfig.Whitelist.Tooltip")]
        public List<ItemDefinition> Whitelist { get; set; } = new()
        {
            // 默认集合
            new ItemDefinition(ItemID.CopperBroadsword),
            new ItemDefinition(ItemID.PalmWoodSword),
            new ItemDefinition(ItemID.WoodenSword),
            new ItemDefinition(ItemID.BorealWoodSword),
            new ItemDefinition(ItemID.CopperBroadsword),
            new ItemDefinition(ItemID.RichMahoganySword),
            new ItemDefinition(ItemID.CactusSword),
            new ItemDefinition(ItemID.TinBroadsword),
            new ItemDefinition(ItemID.LeadBroadsword),
            new ItemDefinition(ItemID.EbonwoodSword),
            new ItemDefinition(ItemID.ShadewoodSword),
            new ItemDefinition(ItemID.IronBroadsword),
            new ItemDefinition(ItemID.SilverBroadsword),
            new ItemDefinition(ItemID.GoldBroadsword),
            new ItemDefinition(ItemID.PlatinumBroadsword),
            new ItemDefinition(ItemID.ZombieArm),
            new ItemDefinition(ItemID.TungstenBroadsword),
            new ItemDefinition(ItemID.AshWoodSword),
            new ItemDefinition(ItemID.Flymeal),
            new ItemDefinition(ItemID.AntlionClaw),
            new ItemDefinition(ItemID.BoneSword),
            new ItemDefinition(ItemID.BatBat),
            new ItemDefinition(ItemID.TentacleSpike),
            new ItemDefinition(ItemID.CandyCaneSword),
            new ItemDefinition(ItemID.Katana),
            new ItemDefinition(ItemID.IceBlade),
            new ItemDefinition(ItemID.LightsBane),
            new ItemDefinition(ItemID.Muramasa),
            new ItemDefinition(ItemID.BloodButcherer),
            new ItemDefinition(ItemID.Starfury),
            new ItemDefinition(ItemID.EnchantedSword),
            new ItemDefinition(ItemID.PurpleClubberfish),
            new ItemDefinition(ItemID.BeeKeeper),
            new ItemDefinition(ItemID.FalconBlade),
            new ItemDefinition(ItemID.BladeofGrass),
            new ItemDefinition(ItemID.NightsEdge),
            new ItemDefinition(ItemID.PearlwoodSword),
            new ItemDefinition(ItemID.DyeTradersScimitar),
            new ItemDefinition(ItemID.TaxCollectorsStickOfDoom),
            new ItemDefinition(ItemID.SlapHand),
            new ItemDefinition(ItemID.CobaltSword),
            new ItemDefinition(ItemID.PalladiumSword),
            new ItemDefinition(ItemID.MythrilSword),
            new ItemDefinition(ItemID.OrichalcumSword),
            new ItemDefinition(ItemID.BreakerBlade),
            new ItemDefinition(ItemID.Cutlass),
            new ItemDefinition(ItemID.Frostbrand),
            new ItemDefinition(ItemID.AdamantiteSword),
            new ItemDefinition(ItemID.BeamSword),
            new ItemDefinition(ItemID.TitaniumSword),
            new ItemDefinition(ItemID.Bladetongue),
            new ItemDefinition(ItemID.HamBat),
            new ItemDefinition(ItemID.WaffleIron),
            new ItemDefinition(ItemID.PsychoKnife),
            new ItemDefinition(ItemID.Keybrand),
            new ItemDefinition(ItemID.ChlorophyteClaymore),
            new ItemDefinition(ItemID.DD2SquireDemonSword),
            new ItemDefinition(ItemID.DD2SquireBetsySword),
            new ItemDefinition(ItemID.StarWrath),
            new ItemDefinition(ItemID.InfluxWaver),
            new ItemDefinition(ItemID.Meowmere),
            new ItemDefinition(ItemID.ChlorophyteSaber),
            new ItemDefinition(ItemID.ChristmasTreeSword),
            new ItemDefinition(ItemID.Seedler),
            // 所有光剑
            new ItemDefinition(ItemID.RedPhaseblade),
            new ItemDefinition(ItemID.BluePhaseblade),
            new ItemDefinition(ItemID.GreenPhaseblade),
            new ItemDefinition(ItemID.YellowPhaseblade),
            new ItemDefinition(ItemID.PurplePhaseblade),
            new ItemDefinition(ItemID.OrangePhaseblade),
            new ItemDefinition(ItemID.WhitePhaseblade),
            new ItemDefinition(ItemID.BluePhasesaber),
            new ItemDefinition(ItemID.PurplePhasesaber),
            new ItemDefinition(ItemID.OrangePhasesaber),
            new ItemDefinition(ItemID.WhitePhasesaber),
            new ItemDefinition(ItemID.PurplePhasesaber),
            new ItemDefinition(ItemID.OrangePhasesaber),
            new ItemDefinition(ItemID.WhitePhasesaber),
        };

        public override void OnLoaded() => BladeTrailRuntime.ApplyFromConfig(this);
        public override void OnChanged() => BladeTrailRuntime.ApplyFromConfig(this);
    }
}