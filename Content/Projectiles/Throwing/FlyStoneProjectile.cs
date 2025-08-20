// FlyStoneProjectile.cs - 飞石射弹
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WuDao.Content.Projectiles.Throwing
{
    public class FlyStoneProjectile : ModProjectile
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Throwing/FlyStone";
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.DamageType = DamageClass.Throwing;

            Projectile.timeLeft = 360;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;

            // 添加抽尾设置
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 100)
            {
                if (Projectile.velocity.X > 1.5f)
                {
                    Projectile.velocity.X -= 0.15f;
                }
                Projectile.velocity.Y += 0.15f; // 模拟重力
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            int hits = 3 - Projectile.penetrate;
            if (hits == 1)
            {
                modifiers.SourceDamage *= 0.75f;
            }
            else if (hits == 2)
            {
                modifiers.SourceDamage *= 0.5f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CreateImpactDust();
        }

        public override void OnKill(int timeLeft)
        {
            CreateImpactDust();
        }

        private void CreateImpactDust()
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 100, default, 0.85f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            // 画拖尾
            for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Vector2 drawPos = Projectile.oldPos[i] + origin - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                Color color = lightColor * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                Main.spriteBatch.Draw(texture, drawPos, null, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }
            // 返回 false 就需要自己手动把本体画出来
            Vector2 currentPos = Projectile.Center - Main.screenPosition;
            Main.spriteBatch.Draw(texture, currentPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
