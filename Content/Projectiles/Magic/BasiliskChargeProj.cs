using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Items.Weapons.Magic
{
    // 魔法号角 随机射弹 石化蜥冲锋
    public class BasiliskChargeProj : ModProjectile
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.DesertBeast}";
        public int FrameTicksPerFrame = 5;
        public float MinSpeed = 8f;
        // private static Asset<Texture2D> TexAsset;
        // public override void Load()
        // {
        //     if (!Main.dedServ)
        //     {
        //         TexAsset = ModContent.Request<Texture2D>(Texture, AssetRequestMode.AsyncLoad);
        //     }
        // }
        // public override void Unload()
        // {
        //     TexAsset = null;
        // }
        public override void SetStaticDefaults()
        {
            // 原版石化蜥有多帧
            Main.projFrames[Projectile.type] = Main.npcFrameCount[NPCID.DesertBeast];
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 40;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180; // 最多存在3秒
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.spriteDirection = -1;
        }

        public override void AI()
        {
            // 计时：ai[0] 累加（60 tick = 1s）
            Projectile.ai[0]++;

            // ===== 0.5 秒后开始弱追踪 =====
            if (Projectile.ai[0] == 30f)
            {
                // 进入追踪的一次性提示：喷尘土便于观察
                for (int k = 0; k < 20; k++)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sandnado, Scale: 2.0f);
            }

            if (Projectile.ai[0] >= 30f)
            {
                const float homingRange = 320f;  // 大范围，便于命中目标
                const float homingLerp = 0.16f;  // 先强一点验证（看到明显拐弯后再降到 0.06~0.10）
                float speed = Projectile.velocity.Length();
                if (speed < MinSpeed) speed = MinSpeed;       // 防止速度被耗光

                NPC target = null;
                float best = homingRange;

                // 找最近可追目标（先不做视线判断，确定功能正常后再加）
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (n.active && !n.friendly && !n.dontTakeDamage && n.CanBeChasedBy())
                    {
                        float d = Vector2.Distance(Projectile.Center, n.Center);
                        if (d < best)
                        {
                            best = d; target = n;
                        }
                    }
                }

                if (target != null)
                {
                    // 期望速度方向（保持原速度幅值）
                    Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * speed;

                    // 线性插值逼近期望方向
                    Vector2 newVel = Vector2.Lerp(Projectile.velocity, desired, homingLerp);

                    // 归一化恢复速度幅值，避免越补越慢
                    if (newVel.LengthSquared() > 0.001f)
                        newVel = newVel.SafeNormalize(Vector2.UnitX) * speed;

                    Projectile.velocity = newVel;
                }
            }

            // 帧动画
            if (++Projectile.frameCounter >= FrameTicksPerFrame)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            // 轻微尘土
            if (Main.rand.NextBool(3))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sandnado);

            // 对准方向
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // Texture2D tex = TexAsset.Value;
            Texture2D tex = TextureAssets.Npc[NPCID.DesertBeast].Value;
            
            int frames = Main.projFrames[Projectile.type];
            int frameHeight = tex.Height / frames;
            Rectangle src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            // 角度跨到左半边时会出现“上下颠倒”的观感（因为整张图被旋转了180°）。
            // 解决：当朝向的 X 分量为负，就竖直翻转一次，让顶朝上、头朝前。
            SpriteEffects fx = (Projectile.velocity.X >= 0f)
                ? SpriteEffects.FlipVertically
                : SpriteEffects.None;

            Vector2 origin = new Vector2(src.Width / 2f, src.Height / 2f);
            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                src,
                lightColor,
                Projectile.rotation,
                origin,
                1f,
                fx,
                0
            );
            return false;
        }
    }
}
