using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Players
{
    // 压制力场
    public class DevolutionPlayer : ModPlayer
    {
        public const float AuraRadius = 16f * 30f; // 30格
        public bool HasDevolutionAura;

        public override void ResetEffects()
        {
            HasDevolutionAura = false;
        }

        public override void PostUpdate()
        {
            if (!HasDevolutionAura || Main.netMode == NetmodeID.Server)
                return;

            // 纯表现，不参与多人判定
            const int dustCount = 6;
            for (int k = 0; k < dustCount; k++)
            {
                float ang = MathHelper.TwoPi * k / dustCount;
                Vector2 pos = Player.Center + AuraRadius * ang.ToRotationVector2();

                int d = Dust.NewDust(pos - new Vector2(4f), 8, 8, DustID.MagicMirror, 0f, 0f, 150, default, 1f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = Vector2.Zero;
            }
        }
    }
}