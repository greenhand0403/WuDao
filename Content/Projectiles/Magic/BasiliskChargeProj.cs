using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace WuDao.Content.Items.Weapons.Magic
{
    public class BasiliskChargeProj : ModProjectile
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.DesertBeast}";

        public override void SetStaticDefaults()
        {
            // 原版石化蜥有多帧
            Main.projFrames[Projectile.type] = Main.npcFrameCount[NPCID.DesertBeast];
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 40;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180; // 最多存在3秒
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // 简易寻的：缓慢转向最近敌怪
            NPC target = null;
            float best = 900f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var n = Main.npc[i];
                if (n.active && !n.friendly && n.CanBeChasedBy())
                {
                    float d = Vector2.Distance(Projectile.Center, n.Center);
                    if (d < best)
                    {
                        best = d; target = n;
                    }
                }
            }

            if (target != null)
            {
                Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 14f;
                // 平滑转向
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.08f);
            }

            Projectile.rotation = Projectile.velocity.X * 0.03f;

            // 帧动画
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            // 轻微尘土
            if (Main.rand.NextBool(3))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sandnado);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 碰砖减穿透并反弹一点
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0) Projectile.Kill();
            if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X * 0.6f;
            if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y * 0.6f;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 以NPC帧表绘制
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frames = Main.projFrames[Projectile.type];
            int frameHeight = tex.Height / frames;
            Rectangle src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 origin = new Vector2(src.Width / 2f, src.Height / 2f);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, src, lightColor, Projectile.rotation, origin, 1f, fx, 0);
            return false;
        }
    }
}
