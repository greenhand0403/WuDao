using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using WuDao.Content.Global.Projectiles;

namespace WuDao.Content.Players
{
    // 近视眼镜 根据距离给玩家增加伤害
    public class NearsightedPlayer : ModPlayer
    {
        public bool Nearsighted;
        public bool ShowRangeRings; // 想关掉可视化就设为 false
        // ===== 真实数值效果：所有端都要算，尤其服务器要算 =====
        readonly float nearRadius = 10 * 16f;
        readonly float midRadius = 20 * 16f;
        readonly float farRadius = 30 * 16f;

        readonly float NearBonusDamage = 0.1f;
        readonly float MidBonusDamage = 0.15f;
        
        readonly float MidEndurance = 0.15f;

        readonly float FarProjReduction = 0.1f;
        readonly float MidProjReduction = 0.15f;

        public override void ResetEffects()
        {
            Nearsighted = false;
            ShowRangeRings = false;
        }
        private void SpawnRingDust(float radius, int dustType)
        {
            const int count = 20;

            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 offset = radius * angle.ToRotationVector2();
                Vector2 pos = Player.Center + offset;

                int d = Dust.NewDust(pos - new Vector2(4f), 8, 8, dustType, 0f, 0f, 140, default, 1f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = Vector2.Zero;
            }
        }
        public override void PostUpdateEquips()
        {
            if (!Nearsighted)
                return;

            bool anyNear = false;
            bool anyMid = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc == null || !npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                float d = Vector2.Distance(Player.Center, npc.Center);

                if (d <= nearRadius)
                {
                    anyNear = true;
                    break;
                }
                else if (d <= midRadius)
                {
                    anyMid = true;
                }
            }

            if (anyNear)
            {
                Player.GetDamage(DamageClass.Generic) += NearBonusDamage;
            }
            else if (anyMid)
            {
                Player.GetDamage(DamageClass.Generic) += MidBonusDamage;
                Player.endurance += MidEndurance;
            }

            // ===== 纯本地表现：范围圆环 =====
            if (!ShowRangeRings)
                return;

            if (Main.dedServ)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            SpawnRingDust(nearRadius, DustID.GemRuby);
            SpawnRingDust(midRadius, DustID.GemEmerald);
            SpawnRingDust(farRadius, DustID.GemDiamond);
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (!Nearsighted)
                return;

            if (proj == null || !proj.active || !proj.hostile)
                return;

            var gp = proj.GetGlobalProjectile<NearsightedGlobalProjectile>();
            int sourceNpcId = gp.SourceNPC;

            if (sourceNpcId < 0 || sourceNpcId >= Main.maxNPCs)
                return;

            NPC sourceNpc = Main.npc[sourceNpcId];
            if (sourceNpc == null || !sourceNpc.active || sourceNpc.friendly)
                return;

            float d = Vector2.Distance(Player.Center, sourceNpc.Center);

            if (d >= farRadius)
            {
                modifiers.FinalDamage *= 1f - FarProjReduction;
            }
            else if (d >= midRadius)
            {
                modifiers.FinalDamage *= 1f - MidProjReduction;
            }
        }
        /// <summary>
        /// 在以 center 为圆心、半径为 radiusPx 的圆上撒一圈无重力 Dust。
        /// 采用低频&稀疏点位，性能友好；通过时间偏移让粒子“缓慢旋转”。
        /// </summary>
        private void SpawnRangeRing(Vector2 center, float radiusPx, int dustId)
        {
            // 每隔 8 帧画一批粒子
            if (Main.GameUpdateCount % 8 != 0) return;

            int points = 16;                       // 本次批次撒 16 个点（足够看清）
            float step = MathHelper.TwoPi / points;
            float t = Main.GlobalTimeWrappedHourly * 0.6f; // 旋转速度（可调）
            float angleOffset = t;                 // 时间偏移让环看起来在“流动”

            for (int i = 0; i < points; i++)
            {
                float ang = angleOffset + i * step;
                Vector2 pos = center + new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * radiusPx;

                // 轻微抖动让视觉更柔和
                pos += Main.rand.NextVector2Circular(1.5f, 1.5f);

                Dust dust = Dust.NewDustPerfect(pos, dustId, Vector2.Zero, 150, default, 0.9f);
                if (dust != null)
                {
                    dust.noGravity = true;
                    dust.scale *= Main.rand.NextFloat(0.85f, 1.15f);
                }
            }
        }
    }
}