using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Global.NPCs
{

    // 全局 NPC：在 NPC 死亡时追加一次或多次“原版掉落表”的执行
    public class DesignFlawGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            // 只在服务端执行掉落逻辑（防止客户端重复）
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (!(npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type])) return;

            int killer = npc.lastInteraction; // 最后造成伤害的玩家索引
            if (killer < 0 || killer >= Main.maxPlayers)
                return;

            Player player = Main.player[killer];
            if (player is null || !player.active)
                return;

            var modPlr = player.GetModPlayer<DesignFlawPlayer>();
            // 条件：饰品正在生效、记录的NPC类型匹配、计数>0
            if (!modPlr.hasFlaw || modPlr.recordedNPCType != npc.type || modPlr.defeatCount <= 0)
                return;

            // 额外掉落次数 = 1 + 被该NPC击败次数
            int times = 1 + modPlr.defeatCount;

            RunVanillaLootTableMultipleTimes(npc, player, times);

            // 清空记录
            modPlr.recordedNPCType = -1;
            modPlr.defeatCount = 0;
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
                rng = Main.rand,                  // 使用全局 RNG；如需更独立，也可 new UnifiedRandom(Main.rand.Next())
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