using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Global.NPCs
{
    // 败笔：击败 BOSS 后获得额外的战利品
    // 全局 NPC：在 NPC 死亡时追加一次或多次“原版掉落表”的执行
    public class DesignFlawGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            // 只在服务端执行掉落逻辑（防止客户端重复）
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (!(npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type])) return;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player == null || !player.active)
                    continue;

                DesignFlawPlayer modPlr = player.GetModPlayer<DesignFlawPlayer>();

                // 当前必须装备着败笔，且记录匹配
                if (!modPlr.hasFlaw || modPlr.recordedNPCType != npc.type || modPlr.defeatCount <= 0)
                    continue;

                int times = modPlr.defeatCount;

                RunVanillaLootTableMultipleTimes(npc, player, times);

                int bossBagItemID = GetBossBagItemIDForNPCType(npc.type);
                if (bossBagItemID > 0)
                {
                    Item.NewItem(
                        npc.GetSource_Death(),
                        npc.position,
                        npc.width,
                        npc.height,
                        bossBagItemID,
                        times
                    );
                }

                // 只清空这个玩家自己的记录
                modPlr.ClearRecord(sync: true);
            }
        }
        // 简易映射（把常用 boss 列出来）
        private static int GetBossBagItemIDForNPCType(int npcType)
        {
            switch (npcType)
            {
                case NPCID.KingSlime: return ItemID.KingSlimeBossBag;
                case NPCID.EyeofCthulhu: return ItemID.EyeOfCthulhuBossBag;
                case NPCID.BrainofCthulhu: return ItemID.BrainOfCthulhuBossBag;
                case NPCID.QueenBee: return ItemID.QueenBeeBossBag;
                case NPCID.SkeletronHead: return ItemID.SkeletronBossBag;
                case NPCID.WallofFlesh: return ItemID.WallOfFleshBossBag;
                case NPCID.QueenSlimeBoss: return ItemID.QueenSlimeBossBag;
                case NPCID.Plantera: return ItemID.PlanteraBossBag;
                case NPCID.Golem: return ItemID.GolemBossBag;
                case NPCID.DukeFishron: return ItemID.FishronBossBag;
                case NPCID.CultistBoss: return ItemID.CultistBossBag;
                case NPCID.MoonLordCore: return ItemID.MoonLordBossBag;
                default:
                    return 0;
            }
        }
        private static void RunVanillaLootTableMultipleTimes(NPC npc, Player owner, int times)
        {
            // 取得该NPC的规则集合（包含它的完整原版掉落）
            // 第二个参数是 includeGlobalDrops：一般为 true，这样全局掉落（如通用奖金/旗帜条件等）也会包含。
            // 如果你只想重跑“该NPC专属规则”，可以传 false。
            List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForNPCID(npc.type, includeGlobalDrops: true);

            // 构造 DropAttemptInfo（驱动 ItemDropRule 的执行）
            DropAttemptInfo MakeInfo() => new DropAttemptInfo
            {
                npc = npc,
                player = owner,
                rng = Main.rand,// 使用全局 RNG；如需更独立，也可 new UnifiedRandom(Main.rand.Next())
                IsExpertMode = Main.expertMode,
                IsMasterMode = Main.masterMode,
                IsInSimulation = false,
                // playerLuck = owner.luck
            };

            for (int i = 0; i < times; i++)
            {
                var info = MakeInfo();
                // 逐条规则尝试掉落；规则内部会负责 Boss Bag、OneFromOptions、条件规则等的完整流程
                foreach (var rule in rules)
                    rule.TryDroppingItem(info);
            }
        }
    }
}