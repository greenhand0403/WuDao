using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WuDao.Common.Players;

namespace WuDao.Content.Projectiles.Ranged
{
    // 霰弹单颗弹丸：穿透 3， 每穿透1个 -10% 最终伤害
    public class TheOutlawPellet : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MeteorShot}";
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.MaxUpdates = 2;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6; // 减轻同帧多次命中
            // Projectile.spriteDirection = Projectile.direction;
        }

        // 用 localAI[0] 记录已穿透次数
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            int pierced = (int)Projectile.localAI[0];
            float scale = MathHelper.Clamp(1f - 0.1f * pierced, 0.1f, 1f);
            modifiers.FinalDamage *= scale;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hit.Crit)
            {
                Main.player[Projectile.owner].GetModPlayer<TheOutlawPlayer>().OnOurGunCrit();
            }
            // 命中一次后，计作一次穿透
            Projectile.localAI[0] += 1f;
        }

        public override void AI()
        {
            // 简单旋转与拖尾效果 贴图朝右 此处需要旋转90°对齐速度的方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90);
            if (Main.rand.NextBool(6))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 120, default, 0.8f);
        }
    }
}
