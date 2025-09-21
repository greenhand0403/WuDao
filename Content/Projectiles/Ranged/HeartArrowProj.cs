using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Projectiles.Ranged
{
    /// <summary>心箭弹幕：自动追踪；命中回血；击杀掉红心</summary>
    public class HeartArrowProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.WoodenArrowFriendly;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
            Projectile.light = 0.2f;
            Projectile.MaxUpdates = 2;
            Projectile.arrow = true;
        }

        private const float HomingLerp = 0.15f; // 追踪转向平滑
        private const float HomingMaxTurnSpeed = 16f; // 速度上限（避免过猛）
        private const float BaseHomingRadiusTiles = 5f; // 基础5格
        private const float BuffedHomingRadiusTiles = 7f; // 饰品加成7格

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<HeartStuffPlayer>();
            // 格子数x16 转换为像素
            float radiusPx = (mp.SoulGemEquipped ? BuffedHomingRadiusTiles : BaseHomingRadiusTiles) * 16f;

            int targetIndex = -1;
            float dMin = radiusPx * radiusPx;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.CanBeChasedBy(this, false))
                {
                    float d2 = Vector2.DistanceSquared(n.Center, Projectile.Center);
                    if (d2 < dMin && Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1))
                    {
                        dMin = d2;
                        targetIndex = i;
                    }
                }
            }

            if (targetIndex != -1)
            {
                Vector2 desired = (Main.npc[targetIndex].Center - Projectile.Center);
                if (desired.Length() > 6f)
                    desired.Normalize();

                desired *= Math.Max(Projectile.velocity.Length(), 10f);
                // 平滑转向
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, HomingLerp);
                // 封顶
                if (Projectile.velocity.Length() > HomingMaxTurnSpeed)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * HomingMaxTurnSpeed;

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var mp = owner.GetModPlayer<HeartStuffPlayer>();

            // 命中回血：基础1；饰品加成为2
            int heal = mp.SoulGemEquipped ? 2 : 1;
            if (heal > 0 && owner.statLife < owner.statLifeMax2)
            {
                owner.statLife = Math.Min(owner.statLife + heal, owner.statLifeMax2);
                owner.HealEffect(heal, true);
            }

            // 击杀必掉红心（仅在本地玩家端生成一次）
            if (target.life <= 0 && Projectile.owner == Main.myPlayer && !target.friendly && !target.townNPC)
            {
                var src = Projectile.GetSource_FromThis();
                Item.NewItem(src, target.getRect(), ItemID.Heart, 1); // 1颗红心
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 小特效
            for (int i = 0; i < 6; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.HeartCrystal, Scale: 1.1f);
                Main.dust[d].velocity *= 1.2f;
            }
            SoundEngine.PlaySound(SoundID.NPCHit5, Projectile.Center);
        }
    }
}