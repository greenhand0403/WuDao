using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures; // TextureAssets

namespace WuDao.Content.Projectiles.Melee
{
    // 使用原版分形剑的贴图，但这是你自己稳定的类型
    public class FirstFractalCloneProj : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FirstFractal}";

        public override void SetStaticDefaults()
        {
            // 拷贝原版帧数/拖尾设置（可选）
            Main.projFrames[Type] = 15;
            // ProjectileID.Sets.TrailCacheLength[Type] = ProjectileID.Sets.TrailCacheLength[ProjectileID.FirstFractal];
            // ProjectileID.Sets.TrailingMode[Type] = ProjectileID.Sets.TrailingMode[ProjectileID.FirstFractal];
        }

        public override void SetDefaults()
        {
            // —— 方式 A：完全不依赖原版 ID（数值手动填，最稳）——
            Projectile.width = 56;
            Projectile.height = 56;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.aiStyle = 0;   // 自己控 AI，更稳

            // —— 如果你想贴得更像原版，也可以用 CloneDefaults —— 
            // Projectile.CloneDefaults(ProjectileID.FirstFractal);
            // Projectile.aiStyle = 0; // 仍建议把 aiStyle 置 0，AI 自己控
        }
        public override void OnSpawn(IEntitySource source)
        {
            // 固定一个好看的中间帧（0..14）
            Projectile.frame = (Projectile.ai[0] >= 0 && Projectile.ai[0] < 15) ? (int)Projectile.ai[0] : 7;
        }

        public override void AI()
        {
            // 简单线性飞行 + 指向速度方向
            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4; // 视图微调，按需要改

            // 轻微发光/粒子（可按需删掉）
            if (Main.rand.NextBool(4))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SilverFlame, 0f, 0f, 150);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 1.0f + Main.rand.NextFloat() * 0.4f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(ProjectileID.FirstFractal);
            Texture2D tex = TextureAssets.Projectile[ProjectileID.FirstFractal].Value; // 原版贴图
                                                                                       // 水平 15 帧，取第 frame 帧
            Rectangle src = tex.Frame(15, 1, frameX: Projectile.frame, frameY: 0);
            Vector2 origin = src.Size() * 0.5f;
            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                src,
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None
            );
            return false;
        }

        public override bool? CanDamage() => true; // 正常伤害
    }
}
