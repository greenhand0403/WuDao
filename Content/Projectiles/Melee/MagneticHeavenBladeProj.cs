using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Melee
{
    /// <summary>
    /// 磁场天刀：从天而降的刀影（可穿墙），落点处溅射粒子并造成伤害。
    /// ai[0], ai[1] = 目标中心坐标（鼠标点）
    /// localAI[0] = 已经存在的 tick，用于渐进旋转/缩放等
    /// </summary>
    public class MagneticHeavenBladeProj : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.BreakerBlade}";
        public override bool? CanDamage() => true;
        public override bool? CanCutTiles() => false; // 纯演出，不砍草
        public override void SetStaticDefaults()
        {
            // 轻微拖影：可选
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;         // 下落过程可多段命中（配合 local NPC hit cooldown）
            Projectile.timeLeft = 90;          // 1.5s 保护时长足够落地
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;    // 穿墙
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10; // 刀影扫过时不要对同一只怪连续帧爆击
            Projectile.scale = 3.0f;
            Projectile.MaxUpdates = 2;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 旋转指向速度方向（下落为主）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
        }

        public override void AI()
        {
            // 目标中心
            Vector2 target = new Vector2(Projectile.ai[0], Projectile.ai[1]);

            // 轻微自转 + 指向目标
            Projectile.localAI[0]++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 逼近目标时稍微加速，落点更干脆
            float dist = Vector2.Distance(Projectile.Center, target);
            if (dist > 40f)
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, (target - Projectile.Center).SafeNormalize(Vector2.UnitY) * 24f, 0.1f);
            }
            else
            {
                // 到达附近，立刻结束并触发爆裂
                Projectile.Kill();
            }

            // 轻微发光粒子尾迹（可选）
            if (Main.rand.NextBool(3))
            {
                var d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), DustID.Electric, Projectile.velocity * 0.1f);
                d.noGravity = true;
                d.scale = 1.1f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 落点音效 + 粒子溅射 + 小范围二段伤害（可选）
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, PitchVariance = 0.2f }, Projectile.Center);

            // 电火花 + 金属碎屑感
            for (int i = 0; i < 30; i++)
            {
                var v = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(6f, 13f);
                int id = Dust.NewDust(Projectile.Center - new Vector2(8, 8), 16, 16, DustID.Electric, v.X, v.Y, 0, default, Main.rand.NextFloat(1.1f, 1.6f));
                Main.dust[id].noGravity = true;
            }
            for (int i = 0; i < 12; i++)
            {
                var v = Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(3f, 7f);
                int id = Dust.NewDust(Projectile.Center - new Vector2(8, 8), 16, 16, DustID.SilverCoin, v.X, v.Y, 100, default, Main.rand.NextFloat(0.9f, 1.2f));
                Main.dust[id].noGravity = true;
            }

            // 可选：在落点制造一次小范围伤害（类似爆裂），半径 96
            int damage = (int)(Projectile.damage * 0.8f);
            float kb = 3f;
            Rectangle box = Utils.CenteredRectangle(Projectile.Center, new Vector2(192, 192));
            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC npc = Main.npc[n];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && box.Intersects(npc.Hitbox))
                {
                    int dir = (npc.Center.X > Projectile.Center.X) ? 1 : -1;
                    npc.StrikeNPC(new NPC.HitInfo
                    {
                        Damage = damage,
                        Knockback = kb,
                        HitDirection = dir,
                        Crit = Main.rand.NextBool(6)
                    }, noPlayerInteraction: false);
                }
            }
        }
    }

    /// <summary>
    /// 磁场“场域”弹幕：不可见、短时存在；持续对范围内敌怪施加吸引并施加阻尼，避免一次性脉冲带来的“继续飘走”问题
    /// ai[0] = 半径
    /// </summary>
    public class MagneticHeavenBladeField : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None; // 不显示

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.timeLeft = 15;         // 短暂吸附
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            float radius = Projectile.ai[0] > 0 ? Projectile.ai[0] : 600f;
            Vector2 center = Projectile.Center;

            // 圆周特效（演出）：每帧几颗尘
            if (Main.rand.NextBool(2))
            {
                Vector2 p = center + Main.rand.NextVector2CircularEdge(radius, radius);
                var d = Dust.NewDustPerfect(p, DustID.GemDiamond, (center - p).SafeNormalize(Vector2.Zero) * 2f, 0, default, 1.2f);
                d.noGravity = true;
            }

            // —— 核心：持续吸附 + 阻尼 —— //
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;

                float dist = Vector2.Distance(npc.Center, center);
                if (dist > radius) continue;

                Vector2 dir = (center - npc.Center);
                float pull = MathHelper.Clamp((radius - dist) / radius, 0f, 1f); // 越靠外吸力越大
                float accel = 1.8f + 3.2f * pull; // 1.8~5.0 像素/tick 的加速度
                Vector2 desiredVel = dir.SafeNormalize(Vector2.Zero) * (8f + 10f * pull);

                // 速度插值朝向中心
                npc.velocity = Vector2.Lerp(npc.velocity, desiredVel, 0.35f);

                // 阻尼：快速衰减“横向/切向”速度，防止穿过中心后继续飘
                npc.velocity *= 0.88f;

                // 当很靠近中心时，几乎停住
                if (dist < 32f)
                    npc.velocity *= 0.5f;
            }
        }
    }
}