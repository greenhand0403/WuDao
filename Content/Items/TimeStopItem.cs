using Terraria;
using Terraria.ID;
using WuDao.Content.Systems;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Items
{
    /// <summary>
    /// 静止游鱼：冻结时间，只有玩家能移动，但是不能攻击
    /// </summary>
    public class TimeStopItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 34;
            Item.useAnimation = 34;
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item4;
        }
        public override bool CanUseItem(Player player)
        {
            // 冷却中禁止使用
            if (TimeStopSystem.IsOnCooldown)
                return false;

            // 冻结中也不允许再次施放
            if (TimeStopSystem.IsFrozen)
                return false;

            return true;
        }
        public override bool? UseItem(Player player)
        {
            const int duration = 300;   // 冻结 5s（60fps）
            const int cooldown = 2400;   // 冷却 2 分钟

            // Global 冻结；如果你要用“飞仙定向冻结”，把 scope/allowed 换成对应参数
            bool ok = TimeStopSystem.TryStartFreeze(duration, cooldown, FreezeScope.Global, -1);
            if (ok)
            {
                // 播个音效/特效（可选）
                // SoundEngine.PlaySound(SoundID.Item, player.Center);
                return true;
            }
            else
            {
                // 可选：给玩家一个提示（战斗文本/聊天）
                // CombatText.NewText(player.Hitbox, Color.Cyan, $"冷却中：{TimeStopSystem.CooldownSeconds}s");
                return false;
            }
        }
    }
}