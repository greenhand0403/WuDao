using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent.ItemDropRules;
using System.Collections.Generic;
// TODO: 换贴图
namespace WuDao.Content.Items.Accessories
{
    /*
        核心机制：
        记录击败玩家的 NPC 类型。
        玩家死亡时，如果是被 NPC 杀死 → 记录该 NPC 类型和次数。
        装备后，对该 NPC 造成 10% × 被击败次数 的额外伤害。
        击杀该 NPC 时，额外掉落 1 + 被击败次数 个物品，并清空记录。
    */
    public class DesignFlaw : ModItem
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("设计败笔");
        //     Tooltip.SetDefault("无初始效果\n记录上一个击败玩家的NPC，并强化对它的伤害");
        // }
        public override string Texture => $"Terraria/Images/Item_{ItemID.Pengfish}";
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DesignFlawPlayer>().hasFlaw = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var mp = Main.LocalPlayer.GetModPlayer<DesignFlawPlayer>();

            if (mp.recordedNPCType > -1 && mp.defeatCount > 0)
            {
                string npcName = Lang.GetNPCNameValue(mp.recordedNPCType);
                tooltips.Add(new TooltipLine(Mod, "DesignFlaw_State", $"记录：{npcName} × {mp.defeatCount}"));
                tooltips.Add(new TooltipLine(Mod, "DesignFlaw_Bonus", $"对其伤害 +{mp.defeatCount * 10}%"));
                tooltips.Add(new TooltipLine(Mod, "DesignFlaw_ResetHint", "击杀该NPC后：按原版规则额外掉落 (1+次数) 次，然后重置记录"));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "DesignFlaw_State", "记录：无（尚未被NPC击败或已重置）"));
                tooltips.Add(new TooltipLine(Mod, "DesignFlaw_Hint", "效果：下次被某个NPC击败后开始记录它，并对其造成额外伤害"));
            }
        }
    }

    public class DesignFlawPlayer : ModPlayer
    {
        public bool hasFlaw;
        public int recordedNPCType = -1;
        public int defeatCount = 0;

        public override void ResetEffects()
        {
            hasFlaw = false;
        }
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (hasFlaw && damageSource.SourceNPCIndex >= 0)
            {
                NPC npc = Main.npc[damageSource.SourceNPCIndex];
                if (npc.active)
                {
                    if (recordedNPCType == -1)
                    {
                        recordedNPCType = npc.type;
                        defeatCount = 1;
                    }
                    else if (recordedNPCType == npc.type)
                    {
                        Main.NewText($" {npc.FullName} 已将你击败 {defeatCount} 次");
                        defeatCount += 1;
                    }
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (hasFlaw && target.type == recordedNPCType && defeatCount > 0)
            {
                modifiers.FinalDamage *= 1f + (0.1f * defeatCount);
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (hasFlaw && target.type == recordedNPCType && defeatCount > 0)
            {
                modifiers.FinalDamage *= 1f + (0.1f * defeatCount);
            }
        }
    }

    // 全局 NPC：在 NPC 死亡时追加一次或多次“原版掉落表”的执行
    public class DesignFlawGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            // 只在服务端执行掉落逻辑（防止客户端重复）
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

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
