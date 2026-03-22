using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Projectiles.Ranged
{
    /// <summary>
    /// 心箭弹幕：自动追踪；命中回血；击杀掉红心
    /// </summary>
    public class HeartArrowProj : ModProjectile
    {
        private const float HomingLerp = 0.15f; // 追踪转向平滑
        private const float HomingMaxTurnSpeed = 16f; // 速度上限（避免过猛）
        private const float BaseHomingRadiusTiles = 5f; // 基础5格
        private const float BuffedHomingRadiusTiles = 7f; // 饰品加成7格
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

        public override void AI()
        {
            bool buffedHoming = Projectile.ai[0] == 1f;
            float radiusPx = (buffedHoming ? BuffedHomingRadiusTiles : BaseHomingRadiusTiles) * 16f;

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
                Vector2 desired = Main.npc[targetIndex].Center - Projectile.Center;
                if (desired.Length() > 6f)
                    desired.Normalize();

                desired *= Math.Max(Projectile.velocity.Length(), 10f);
                // 平滑转向
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, HomingLerp);
                // 封顶
                if (Projectile.velocity.Length() > HomingMaxTurnSpeed)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * HomingMaxTurnSpeed;

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                // 追踪方向变化比较明显时，通知同步
                // if (Main.netMode != NetmodeID.SinglePlayer)
                //     Projectile.netUpdate = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            bool buffedHoming = Projectile.ai[0] == 1f;

            // 只让拥有者实际回血，避免多端重复改血
            if (Projectile.owner == Main.myPlayer)
            {
                int heal = buffedHoming ? 2 : 1;
                if (heal > 0 && owner.statLife < owner.statLifeMax2)
                {
                    owner.Heal(heal);
                }
            }

            // 掉红心应交给服务器或单机执行，避免客户端各掉一份
            if (target.life <= 0 && !target.friendly && !target.townNPC)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Item.NewItem(Projectile.GetSource_FromThis(), target.getRect(), ItemID.Heart, 1);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ)
                return;

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