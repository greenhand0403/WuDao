using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Ranged
{
    public class RainbowArrowProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // 拖尾长度
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            // 记录旧位置的模式，0 就够用了
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;

            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.WoodenArrowFriendly;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = 1;
            Projectile.timeLeft = 1200;
            Projectile.arrow = true;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;

            Projectile.extraUpdates = 0;

            // 原版字段，Projectile 有 alpha / scale / rotation 等绘制相关属性
            // 这里初始给一点透明感，飞行中再逐渐稳定
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 给每支箭一个不同的初始相位，避免多箭完全同步变色
            Projectile.localAI[0] = Main.rand.NextFloat();
        }

        public override void AI()
        {
            // 箭朝向速度方向
            if (Projectile.velocity.LengthSquared() > 0.001f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            // localAI[0] 作为色相偏移
            // localAI[1] 作为一个额外计时器
            Projectile.localAI[1] += 0.02f;

            // 每隔几帧产生一个彩虹粒子
            if (Main.rand.NextBool(2))
            {
                SpawnRainbowDust();
            }

            // 偶尔再补一个更亮的小粒子，让发光感更强
            if (Main.rand.NextBool(4))
            {
                SpawnGlowDust();
            }

            // 给箭提供一点动态光照
            Color lightColor = GetRainbowColor(0f);
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.45f);
        }

        private Color GetRainbowColor(float offset)
        {
            // Main.GlobalTimeWrappedHourly 是做循环色相最常见的时间源
            float hue = (Main.GlobalTimeWrappedHourly * 0.6f + Projectile.localAI[0] + Projectile.localAI[1] + offset) % 1f;

            // hslToRgb 可以直接做出平滑彩虹色
            Color color = Main.hslToRgb(hue, 1f, 0.6f);

            // 稍微往白色插值一点，更像“发光彩虹”而不是纯色块
            return Color.Lerp(color, Color.White, 0.18f);
        }

        private void SpawnRainbowDust()
        {
            Vector2 dustPos = Projectile.Center - Projectile.velocity * 0.25f;
            int dust = Dust.NewDust(
                dustPos - new Vector2(4f),
                8,
                8,
                DustID.RainbowTorch,
                0f,
                0f,
                100,
                default,
                1.0f
            );

            Dust d = Main.dust[dust];
            d.noGravity = true;
            d.velocity = -Projectile.velocity * 0.18f + Main.rand.NextVector2Circular(0.6f, 0.6f);
            d.scale = Main.rand.NextFloat(0.9f, 1.3f);
            d.fadeIn = 1.05f;

            // 让每个粒子颜色略有偏移，看起来更自然
            d.color = GetRainbowColor(Main.rand.NextFloat(-0.12f, 0.12f));

            // Dust.color 对低饱和/白色贴图很适合做染色
            // 这里正是利用这个机制
        }

        private void SpawnGlowDust()
        {
            int dust = Dust.NewDust(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.GemDiamond,
                0f,
                0f,
                150,
                default,
                0.8f
            );

            Dust d = Main.dust[dust];
            d.noGravity = true;
            d.velocity = Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.4f, 0.4f);
            d.scale = Main.rand.NextFloat(0.7f, 1.0f);
            d.color = GetRainbowColor(Main.rand.NextFloat(-0.08f, 0.08f));
        }

        public override void OnKill(int timeLeft)
        {
            // 死亡时喷一圈彩虹粒子
            for (int i = 0; i < 16; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2.8f, 2.8f);

                int dust = Dust.NewDust(
                    Projectile.Center - new Vector2(4f),
                    8,
                    8,
                    DustID.RainbowTorch,
                    velocity.X,
                    velocity.Y,
                    100,
                    default,
                    Main.rand.NextFloat(1.0f, 1.5f)
                );

                Dust d = Main.dust[dust];
                d.noGravity = true;
                d.color = GetRainbowColor(i / 16f);
            }

            for (int i = 0; i < 8; i++)
            {
                int dust = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.GemDiamond,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    150,
                    default,
                    Main.rand.NextFloat(0.8f, 1.2f)
                );

                Dust d = Main.dust[dust];
                d.noGravity = true;
                d.color = GetRainbowColor(Main.rand.NextFloat());
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.15f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Size() / 2f;

            // 先画拖尾
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Vector2 oldPos = Projectile.oldPos[i];
                if (oldPos == Vector2.Zero)
                    continue;

                float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;

                // 越靠近当前帧越亮
                float opacity = progress * 0.6f;

                // 每一节拖尾给一个不同的色相偏移
                Color trailColor = GetRainbowColor(i * 0.06f) * opacity;

                // 稍微放大一点，制造柔和拖尾
                float scale = Projectile.scale * (0.88f + progress * 0.22f);

                Vector2 drawPos = oldPos + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    frame,
                    trailColor,
                    Projectile.rotation,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }

            // 再画一层“外发光”
            Color glowColor = GetRainbowColor(0f) * 0.55f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(0f, 2f).RotatedBy(MathHelper.TwoPi * i / 4f);

                Main.EntitySpriteDraw(
                    texture,
                    Projectile.Center - Main.screenPosition + offset,
                    frame,
                    glowColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale * 1.08f,
                    SpriteEffects.None,
                    0
                );
            }

            // 最后画箭本体
            Color bodyColor = GetRainbowColor(0f);

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                bodyColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            // 自己绘制完成，阻止原版再次绘制
            return false;
        }
    }
}