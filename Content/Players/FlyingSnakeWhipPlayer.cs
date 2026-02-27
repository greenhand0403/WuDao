using Terraria;
using Terraria.ModLoader;
using WuDao.Content.Items.Weapons.Summon;

namespace WuDao.Content.Players
{
    // 飞蛇鞭 蛇鞭
    public class FlyingSnakeWhipPlayer : ModPlayer
    {
        // —— 手持鞭子标记（每tick会清空，由 HoldItem 持续置 true）
        public bool HoldingFlyingSnakeWhip;

        // —— 命中后临时生命再生（衰减）
        private int regenTimer;
        private const int RegenMax = 240; // 3秒（60 tick = 1秒）

        public override void ResetEffects()
        {
            // ★关键：不要等 HoldItem 才置 true，直接检测手持物品，保证这一 tick UpdateEquips 就能吃到
            HoldingFlyingSnakeWhip = Player.HeldItem.type == ModContent.ItemType<FlyingSnakeWhip>();
        }

        public override void PostUpdate()
        {
            if (regenTimer > 0)
                regenTimer--;
        }

        // 给鞭子射弹调用：刷新再生计时
        public void TriggerRegen()
        {
            regenTimer = RegenMax;
        }

        public override void UpdateEquips()
        {
            if (HoldingFlyingSnakeWhip)
            {
                // 手持：提升召唤伤害（仆从/召唤弹都吃这个）
                Player.GetDamage(DamageClass.Summon) += 0.15f; // +15%，你可改
            }
        }

        public override void UpdateLifeRegen()
        {
            if (regenTimer <= 0)
                return;

            float t = regenTimer / (float)RegenMax;
            int bonus = (int)(24f * t * t); // ★稍微提高一点，先让你更容易观察

            // ★如果玩家处于负再生状态（中毒/着火等），先把负数拉回 0，避免“看不出来”
            if (Player.lifeRegen < 0)
                Player.lifeRegen = 0;

            if (bonus > 0)
            {
                Player.lifeRegen += bonus;
                Player.lifeRegenTime = 2;
            }
        }

        // 给仆从读的速度倍率（手持就快一点）
        public float MinionSpeedMult => HoldingFlyingSnakeWhip ? 1.25f : 1f; // +25%速度
    }
}