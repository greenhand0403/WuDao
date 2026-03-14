using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Items.Accessories;

namespace WuDao.Content.Global.NPCs
{
    public class LostSymbolGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // 给所有NPC都注册规则，但只有满足条件的才会真正掉落
            npcLoot.Add(
                new LeadingConditionRule(new UndergroundEnemyDropCondition())
                    .OnSuccess(
                        ItemDropRule.OneFromOptions(
                            chanceDenominator: 800, // 0.125%
                            ModContent.ItemType<LostSymbol>(),
                            ModContent.ItemType<LostSymbol1>(),
                            ModContent.ItemType<LostSymbol2>(),
                            ModContent.ItemType<LostSymbol3>(),
                            ModContent.ItemType<LostSymbol4>(),
                            ModContent.ItemType<LostSymbol5>()
                        )
                    )
            );
        }
    }

    public class UndergroundEnemyDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            NPC npc = info.npc;

            // 必须是有效敌怪
            if (npc == null || !npc.active)
                return false;

            // 不让友好NPC、城镇NPC掉
            if (npc.friendly || npc.townNPC)
                return false;

            // 雕像怪通常不建议掉这种物品
            if (npc.SpawnedFromStatue)
                return false;

            if (npc.boss)
                return false;

            // 地下层 + 洞穴层都掉
            float tileY = npc.Center.Y / 16f;
            if (tileY <= Main.worldSurface)
                return false;

            return true;
        }

        public bool CanShowItemDropInUI() => true;

        public string GetConditionDescription()
        {
            return Language.GetTextValue("Mods.WuDao.Item.LostSymbol.Condition");
        }
    }
}