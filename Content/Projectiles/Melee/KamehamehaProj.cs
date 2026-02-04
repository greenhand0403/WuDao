using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Projectiles.Melee
{
    // 龟派气功 射弹
    public class KamehamehaProj : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.StarWrath}";

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.scale = 3.0f;
            Projectile.MaxUpdates = 2;

            // 便于表现与同步
            Projectile.netImportant = true;
            Projectile.light = 0.9f;
        }

        // ai[0] : spentQi（蓄力步数），由发射代码传入（见下面 Kamehameha.cs 修改）
        // ai[1] : internal timer
        public override void OnSpawn(IEntitySource source)
        {
            // 如果有传入蓄力信息，按比例调整初始 scale / timeLeft /速度微调
            int spentQi = (int)Projectile.ai[0];
            if (spentQi < 0) spentQi = 0;
            float extraScale = 0.01f * spentQi; // spentQi 越大，弹幕更粗
            Projectile.scale = 1f + extraScale;
            // Projectile.timeLeft += 10 * spentQi; // 蓄力越多飞行越久

            // 旋转指向速度方向（美观）
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }

        public override void AI()
        {
            // ★ 虚影模式：ai[1] == 1f
            if (Projectile.ai[1] == 1f)
            {
                Player owner = Main.player[Projectile.owner];
                if (!owner.active)
                {
                    Projectile.Kill();
                    return;
                }

                // 贴着玩家中心
                Projectile.Center = owner.MountedCenter;// + Vector2.UnitX * 8;

                // 永远朝鼠标
                Vector2 toMouse = Main.MouseWorld - owner.Center;
                if (toMouse != Vector2.Zero)
                    Projectile.rotation = toMouse.ToRotation() - MathHelper.PiOver2;

                // 规模随蓄力增长（读当前玩家正在累计的 ChargeQiSpent，实时变化）
                var qi = owner.GetModPlayer<QiPlayer>();
                float extraScale = 0.01f * qi.ChargeQiSpent; // 每20点≈+20%
                Projectile.scale = 1f + extraScale;

                // 作为“虚影”：不可伤害、无碰撞、短命续帧
                Projectile.friendly = true;
                Projectile.hostile = false;
                Projectile.timeLeft = 2;
                Projectile.tileCollide = false;
                Projectile.ignoreWater = true;

                // 可选：淡一点
                Projectile.alpha = 30;

                return; // 不跑下面真正飞行的 AI
            }

            // 稳定方向（微小修正），并产生粒子与光
            // Projectile.ai[1] += 1f;

            // 让光线稍微闪动
            float flicker = 0.08f * (float)System.Math.Sin(Main.time * 0.1f);
            Projectile.light = 0.8f + flicker;

            // 稍微保持朝向运动方向
            // if (Projectile.velocity.Length() > 0.1f)
            // {
            //     float wanted = Projectile.velocity.ToRotation();
            //     Projectile.rotation = MathHelper.Lerp(Projectile.rotation, wanted, 0.12f);
            // }

            // 每帧生成少量粒子作为尾迹（不影响性能）
            if (Main.rand.NextBool(2))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FireworkFountain_Pink, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 150, default, 1.15f);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 0.8f + 0.25f * (float)Main.rand.NextDouble();
            }
            // 大一圈光环，间隔产生（表现用）
            // if (Projectile.ai[1] % 8f == 0f)
            // {
            //     for (int i = 0; i < 2; i++)
            //     {
            //         Vector2 vel = Projectile.velocity.RotatedByRandom(0.6f) * (0.2f + Main.rand.NextFloat() * 0.6f);
            //         int d2 = Dust.NewDust(Projectile.Center - new Vector2(8, 8), 16, 16, DustID.GoldFlame, vel.X, vel.Y, 100, default, 1.2f);
            //         Main.dust[d2].noGravity = true;
            //     }
            // }

            // 逐渐微增速度（表现“气”推进感）
            // if (Projectile.ai[1] % 30f == 0f)
            // {
            //     Projectile.velocity *= 1.01f;
            // }

            // 避免无限远：如果距原点非常远则提前销毁（防止被卡住）
            // if (Projectile.Distance(Main.player[Projectile.owner].Center) > 4000f)
            // {
            //     Projectile.Kill();
            // }
        }

        // 命中 NPC 时效果
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 造成额外小范围冲击（视觉）：生成少量火花
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = (Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 6f));
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, vel.X, vel.Y, 200, default, 1.1f);
                Main.dust[d].noGravity = true;
            }

            // 造成短暂击退（根据 owner 的方向）
            int dir = Projectile.direction;
            target.velocity += new Vector2(dir * 3.2f, -2.0f);

            // 如果需要一些特殊 debuff（示例：点燃），可以在这里添加
            // target.AddBuff(BuffID.OnFire, 60);
        }

        // 如果你希望对 tile 发生碰撞让弹幕停下/爆炸，可打开以下 OnTileCollide：
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 保持穿墙效果：直接忽略碰撞（返回 false 表示不销毁）
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            // 跳过虚影射弹
            if (Projectile.ai[1] == 1f)
            {
                return;
            }
            // 爆炸性的结尾表现（不额外造成伤害，纯视觉，若要伤害可改为 true 且 spawn 新 proj）
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            for (int i = 0; i < 18; i++)
            {
                Vector2 vel = (Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 6f));
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, vel.X, vel.Y, 150, default, 1.4f);
                Main.dust[d].noGravity = true;
            }
        }
        public override bool? CanDamage()
        {
            // 虚影模式不造成伤害
            if (Projectile.ai[1] == 1f) return false;
            return true;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[ProjectileID.StarWrath].Value;
            Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                null,
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None
            );
            return false; // 跳过默认的左上角原点绘制
        }

    }
}
