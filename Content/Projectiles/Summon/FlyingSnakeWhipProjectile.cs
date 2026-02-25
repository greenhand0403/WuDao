using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Projectiles.Summon
{
    public class FlyingSnakeWhipProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 20;
            Projectile.WhipSettings.RangeMultiplier = 1.2f;
        }
        private void DrawLine(List<Vector2> list)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, 2);

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
                Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new();
            Projectile.FillWhipControlPoints(Projectile, list);

            DrawLine(list); // 仍然用 FishingLine 画连线

            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 pos = list[0];

            for (int i = 0; i < list.Count - 1; i++)
            {
                // ——对应你 10×92 贴图的默认切片
                Rectangle frame = new Rectangle(0, 0, 10, 26); // 手柄
                Vector2 origin = new Vector2(5, 8);            // 手握点（你可按贴图微调）
                float scale = 1f;

                if (i == list.Count - 2)
                {
                    // 鞭梢（10×18，从 Y=74 开始）
                    frame.Y = 74;
                    frame.Height = 18;

                    // 鞭梢伸展时放大（可选，但很像原版/示例）
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out _, out _);
                    float t = Projectile.ai[0] / timeToFlyOut; // Timer 就是 ai[0]
                    scale = MathHelper.Lerp(
                        0.5f, 1.5f,
                        Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true)
                    );
                }
                else if (i > 10)
                {
                    frame.Y = 58; frame.Height = 16; // 段3
                }
                else if (i > 5)
                {
                    frame.Y = 42; frame.Height = 16; // 段2
                }
                else if (i > 0)
                {
                    frame.Y = 26; frame.Height = 16; // 段1
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;
                float rotation = diff.ToRotation() - MathHelper.PiOver2; // 贴图朝下
                Color color = Lighting.GetColor(element.ToTileCoordinates());

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                pos += diff;
            }

            return false; // 我们自己画完了
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.Poisoned, 120);
            }
            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.Bleeding, 120);
            }
            Player owner = Main.player[Projectile.owner];
            owner.GetModPlayer<FlyingSnakeWhipPlayer>().TriggerRegen(); // ★命中触发再生

            owner.MinionAttackTargetNPC = target.whoAmI;

            Projectile.damage = (int)(Projectile.damage * 0.5f);
        }
    }
}
