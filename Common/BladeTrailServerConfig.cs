using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace WuDao.Common
{
    public class BladeTrailServerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        public override LocalizedText DisplayName =>
            Language.GetText("Mods.WuDao.Configs.BladeTrailServerConfig.DisplayName");

        [Header("$Mods.WuDao.Configs.BladeTrailServerConfig.Headers.DefaultSets")]
        [LabelKey("$Mods.WuDao.Configs.BladeTrailServerConfig.IncludeDefaultBladeTrailSet.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailServerConfig.IncludeDefaultBladeTrailSet.Tooltip")]
        [DefaultValue(true)]
        public bool IncludeDefaultBladeTrailSet { get; set; } = true;

        [LabelKey("$Mods.WuDao.Configs.BladeTrailServerConfig.IncludePhaseblades.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailServerConfig.IncludePhaseblades.Tooltip")]
        [DefaultValue(true)]
        public bool IncludePhaseblades { get; set; } = true;

        [Header("$Mods.WuDao.Configs.BladeTrailServerConfig.Headers.CustomWhitelist")]
        [LabelKey("$Mods.WuDao.Configs.BladeTrailServerConfig.Whitelist.Label")]
        [TooltipKey("$Mods.WuDao.Configs.BladeTrailServerConfig.Whitelist.Tooltip")]
        [DefaultValue(true)]
        public bool EnableWhitelistBladeTrail { get; set; } = true;
        // [LabelKey("$Mods.WuDao.Configs.BladeTrailServerConfig.Whitelist.Label")]
        // [TooltipKey("$Mods.WuDao.Configs.BladeTrailServerConfig.Whitelist.Tooltip")]

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
            new ItemDefinition(ItemID.WhitePhasesaber)
        };

        public override void OnLoaded() => BladeTrailRuntime.ApplyServerConfig(this);
        public override void OnChanged() => BladeTrailRuntime.ApplyServerConfig(this);

        [System.Obsolete]
        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
        {
            // 最小方案：先允许服务器接受改动
            // 以后你想限制只有主机能改，再补权限判断
            return true;
        }
    }
}