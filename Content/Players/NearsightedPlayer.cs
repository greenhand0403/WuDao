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

        public override void ResetEffects()
        {
            Nearsighted = false;
            ShowRangeRings = false;
        }

        public override void PostUpdateEquips()
        {
            if (!Nearsighted || !ShowRangeRings) return;
            if (Main.dedServ) return;
            if (Player.whoAmI != Main.myPlayer) return;

            float minDist = float.MaxValue;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.lifeMax > 5)
                {
                    float dist = Vector2.Distance(npc.Center, Player.Center) / 16f; // tile为单位
                    if (dist < minDist) minDist = dist;
                }
            }

            if (minDist == float.MaxValue) return;

            if (minDist < 8f)
            {
                // +10% 伤害
                Player.GetDamage(DamageClass.Generic) += 0.10f;
            }
            else if (minDist <= 32f)
            {
                // +15% 伤害
                Player.GetDamage(DamageClass.Generic) += 0.15f;
                // -15% 来自射弹的伤害
                Player.endurance += 0.15f;
            }

            // 1 tile = 16 像素
            SpawnRangeRing(Player.Center, 8 * 16, DustID.GemEmerald);   // 8格：绿色
            SpawnRangeRing(Player.Center, 32 * 16, DustID.GemSapphire); // 32格：蓝色
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (!Nearsighted) return;

            // 只处理敌对射弹
            if (proj.friendly) return;

            // 拿到 GlobalProjectile 里记录的发射者
            var gp = proj.GetGlobalProjectile<NearsightedGlobalProjectile>();
            if (gp == null || gp.SourceNPC < 0)
            {
                // 找不到发射者：不做误判，直接忽略（也可选做 >32 的10%，但更安全是忽略）
                return;
            }

            NPC shooter = Main.npc[gp.SourceNPC];
            if (!shooter.active || shooter.friendly) return;

            // 玩家与“发射者NPC”的距离（tile）
            float distTiles = Vector2.Distance(Player.Center, shooter.Center) / 16f;

            float dr = 0f; // 本饰品对该射弹的减伤比例
            if (distTiles >= 8f && distTiles <= 32f)
            {
                dr = 0.15f; // 中距：-15% 射弹伤害
            }
            else if (distTiles > 32f)
            {
                dr = 0.10f; // 远距：-10% 射弹伤害
            } // 近距 <8f：不减伤

            if (dr > 0f)
            {
                modifiers.FinalDamage *= (1f - dr);
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