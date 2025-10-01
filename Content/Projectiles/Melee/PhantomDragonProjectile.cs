using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures; // TextureAssets

namespace WuDao.Content.Projectiles.Melee
{
    public class PhantomDragonProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetStaticDefaults()
        {
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10; // 10帧同一NPC只吃一次
        }
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1; // 贯穿
            Projectile.timeLeft = 210;
            Projectile.aiStyle = 0;
            Projectile.hostile = false;
            Projectile.light = 0.5f;
            Projectile.DamageType = DamageClass.Melee;
        }
        // 修正碰撞体位置
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Rectangle rectangle = new Rectangle(hitbox.X + hitbox.Width / 2, hitbox.Y - hitbox.Height / 2, hitbox.Width, hitbox.Height);
            hitbox = rectangle;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X >= 0 ? 1 : -1;
                Projectile.ai[0] = 1;
            }

            // Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 方向/位置与 PreDraw 同步
            Vector2 dir = Projectile.velocity.LengthSquared() > 0.001f
                ? Vector2.Normalize(Projectile.velocity)
                : new Vector2(Projectile.direction, 0f);
            Vector2 step = -dir * 6f; // 跟 PreDraw 的 spacing 保持一致
            Vector2 segPos = Projectile.Center;

            float hitRadius = 12f; // 每段的“判定粗细”，按你的贴图大小调
            for (int i = 0; i < 4; i++)
            {
                // AABB
                if (Collision.CheckAABBvAABBCollision(targetHitbox.Center(), targetHitbox.Size(), segPos, new Vector2(hitRadius, hitRadius)))
                    return true;
                segPos += step;
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(ProjectileID.LunaticCultistPet);
            Texture2D tex = TextureAssets.Projectile[ProjectileID.LunaticCultistPet].Value;

            // 方向/旋转（速度为0时兜底）
            Vector2 dir = Projectile.velocity.LengthSquared() > 0.001f
                ? Vector2.Normalize(Projectile.velocity)
                : new Vector2(Projectile.direction, 0f);
            float rot = dir.ToRotation();
            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            // 幻影龙帧尺寸40x46，中间躯干部分高度只有20
            // 以“头”为起点，后续各段按 spacing 反向排布
            Vector2 segPos = Projectile.Top;

            for (int i = 0; i < 4; i++)
            {
                Rectangle frame = new Rectangle(0, i * 46, tex.Width, 46);
                // 特殊处理最后一段尾巴，提前补上后腿
                if (i == 3)
                {
                    Rectangle frame1 = new Rectangle(0, 1 * 46, tex.Width, 46);
                    Vector2 origin1 = new Vector2(frame1.Width / 2f, 0);

                    Vector2 drawPos1 = segPos - Main.screenPosition - dir * i * 20;
                    // 注意：左向时加 PI，贴图朝向才一致
                    float drawRot1 = rot + MathHelper.PiOver2;
                    Main.EntitySpriteDraw(tex, drawPos1, frame1, Color.White, drawRot1, origin1, 1f, fx, 0);
                    i = 4;
                }
                Vector2 origin = new Vector2(frame.Width / 2f, 0);

                Vector2 drawPos = segPos - Main.screenPosition - dir * i * 20;
                // 注意：左向时加 PI，贴图朝向才一致
                float drawRot = rot + MathHelper.PiOver2;
                Main.EntitySpriteDraw(tex, drawPos, frame, Color.White, drawRot, origin, 1f, fx, 0);
            }
            return false; // 禁用默认绘制
        }
    }
}