using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Players
{
    public class DevolutionPlayer : ModPlayer
    {
        public bool HasDevolutionAura;

        public override void ResetEffects()
        {
            HasDevolutionAura = false; // 每帧重置
        }
        public override void PostUpdate()
        {
            // 测试用，画圈表示作用范围
            if (HasDevolutionAura && Main.netMode != NetmodeID.Server)
            {
                const float radius = 16f * 30;
                for (int k = 0; k < 12; k++)
                {
                    float ang = MathHelper.TwoPi * k / 12f;
                    Vector2 p = Player.Center + radius * ang.ToRotationVector2();
                    int d = Dust.NewDust(p - new Vector2(4), 8, 8, DustID.MagicMirror, 0, 0, 150, default, 1f);
                    Main.dust[d].noGravity = true;
                }
            }
        }
    }
}