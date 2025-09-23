using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using WuDao.Content.Global;
using WuDao.Content.Items.Accessories;

namespace WuDao.Content.Players
{
    /// <summary>
    /// 参考 ExampleShield 的 ExampleDashPlayer 写法，做“葱盾冲刺+无敌帧”，并用美味值放大冲刺时长。  :contentReference[oaicite:5]{index=5}
    /// </summary>
    public class ScallionDashPlayer : ModPlayer
    {
        // 与 ExampleDashPlayer 一致的方向索引：  :contentReference[oaicite:6]{index=6}
        public const int DashDown = 0;
        public const int DashUp = 1;
        public const int DashRight = 2;
        public const int DashLeft = 3;

        public bool ScallionShieldEquipped;
        public int DashDir = -1;
        public int DashDelay = 0; // 冷却计时
        public int DashTimer = 0; // 当前冲刺剩余帧

        public override void ResetEffects()
        {
            ScallionShieldEquipped = false;

            // —— 双击检测（与示例一致）——  :contentReference[oaicite:7]{index=7}
            if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[DashDown] < 15)
            {
                DashDir = DashDown;
            }
            else if (Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[DashUp] < 15)
            {
                DashDir = DashUp;
            }
            else if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15 && Player.doubleTapCardinalTimer[DashLeft] == 0)
            {
                DashDir = DashRight;
            }
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15 && Player.doubleTapCardinalTimer[DashRight] == 0)
            {
                DashDir = DashLeft;
            }
            else
            {
                DashDir = -1;
            }
        }

        public override void PreUpdateMovement()
        {
            if (CanUseDash() && DashDir != -1 && DashDelay == 0)
            {
                // —— 设置冲刺速度（与示例保持同风格）——  :contentReference[oaicite:8]{index=8}
                Vector2 newVelocity = Player.velocity;
                // 让“美味值”同时影响冲刺速度（而不仅是距离）
                float extra = MathHelper.Clamp(Player.GetModPlayer<CuisinePlayer>().Deliciousness * CuisineGlobalItem.PerDeliciousPointToBonus, 0f, 2f);
                float dashVel = ScallionShield.BaseDashVelocity * (1f + extra);

                switch (DashDir)
                {
                    case DashUp when Player.velocity.Y > -dashVel:
                    case DashDown when Player.velocity.Y < dashVel:
                        {
                            float dir = DashDir == DashDown ? 1f : -1.3f;
                            newVelocity.Y = dir * dashVel;
                            break;
                        }
                    case DashLeft when Player.velocity.X > -dashVel:
                    case DashRight when Player.velocity.X < dashVel:
                        {
                            float dir = DashDir == DashRight ? 1f : -1f;
                            newVelocity.X = dir * dashVel;
                            break;
                        }
                    default:
                        return;
                }

                // —— 开始冲刺 —— 
                DashDelay = ScallionShield.BaseDashCooldown;
                DashTimer = ScallionShield.GetDurationWithDelicious(Player); // ☆ 用美味值放大时长 → 距离更远、i-frame更久
                Player.velocity = newVelocity;

                // 初始一瞬间可以播个尘埃/音效（可选）
                // for (int i = 0; i < 10; i++) Dust.NewDust(Player.position, Player.width, Player.height, DustID.Grass); 
            }

            if (DashDelay > 0)
                DashDelay--;

            if (DashTimer > 0)
            {
                // —— 冲刺“进行中”效果 —— 
                Player.eocDash = DashTimer;                     // 复用克苏鲁之盾的拖影  :contentReference[oaicite:9]{index=9}
                Player.armorEffectDrawShadowEOCShield = true;

                // —— 冲刺期间无敌帧（关键：每帧维持）——
                Player.immune = true;
                Player.immuneNoBlink = true;
                // immuneTime 是倒计时，这里维持一个>=2 的时间，确保每帧都“顶起”
                if (Player.immuneTime < 2) Player.immuneTime = 2;

                DashTimer--;
            }
        }

        private bool CanUseDash()
        {
            // 与示例一致的排他条件：不给原版/坐骑抢  :contentReference[oaicite:10]{index=10}
            return ScallionShieldEquipped
                && Player.dashType == DashID.None
                && !Player.setSolar
                && !Player.mount.Active;
        }
    }
}