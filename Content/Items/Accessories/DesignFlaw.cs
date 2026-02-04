using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent.ItemDropRules;
using System.Collections.Generic;
using WuDao.Content.Global.Projectiles;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    /*
        核心机制：
        记录击败玩家的 NPC 类型。
        玩家死亡时，如果是被 NPC 杀死 → 记录该 NPC 类型和次数。
        装备后，对该 NPC 造成 10% × 被击败次数 的额外伤害。
        击杀该 NPC 时，额外掉落 1 + 被击败次数 个物品，并清空记录。
    */
    /// <summary>
    /// 败笔 饰品
    /// </summary>
    public class DesignFlaw : ModItem
    {
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
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BlackInk)
                .AddIngredient(ItemID.Feather)
                .AddIngredient(ItemID.BambooBlock)
                .AddIngredient(ItemID.ManaCrystal)
                .AddIngredient(ItemID.Book)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
