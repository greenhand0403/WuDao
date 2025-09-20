using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Throwing
{
    public class FoodRainProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.CookedFish; // 占位，不会用这张

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = false;
            Projectile.hostile = true;           // 默认按“红弹”处理；绿弹会在 AI 里关闭
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.aiStyle = 0;
        }

        int ItemType => (int)Projectile.ai[1];
        bool IsRed => Projectile.ai[0] > 0.5f;

        public override void AI()
        {
            // 简单下落 + 微随机
            Projectile.rotation += 0.05f * Projectile.direction;
            Projectile.velocity.X *= 0.99f;

            // 绿弹：对玩家无害，手动检测碰撞→治疗
            if (!IsRed)
            {
                Projectile.hostile = false;
                // 与本地玩家碰撞即治疗
                var plr = Main.LocalPlayer;
                if (plr.active && !plr.dead && Projectile.Hitbox.Intersects(plr.Hitbox))
                {
                    int heal = 10; // 绿弹回血
                    plr.statLife = Utils.Clamp(plr.statLife + heal, 0, plr.statLifeMax2);
                    plr.HealEffect(heal, true);
                    Projectile.Kill();
                }
            }

            // 外圈光效（红/绿）
            Lighting.AddLight(Projectile.Center, IsRed ? new Vector3(0.8f, 0.1f, 0.1f) : new Vector3(0.1f, 0.8f, 0.1f));
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => true;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // 红弹打到玩家掉血，无额外效果
        }
        public override void OnSpawn(IEntitySource source)
        {
            Main.instance.LoadItem(ItemType);
            base.OnSpawn(source);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var tex = Terraria.GameContent.TextureAssets.Item[ItemType].Value;

            Vector2 pos = Projectile.Center - Main.screenPosition;

            // ★ 仅绘制第 1 帧
            Rectangle? src = null;
            var anim = Main.itemAnimations[ItemType];
            if (anim is DrawAnimationVertical v)
            {
                int frameHeight = tex.Height / v.FrameCount;
                src = new Rectangle(0, 0, tex.Width, frameHeight);
            }

            var glowColor = IsRed ? Color.Red : Color.Green;
            glowColor.A = 100;
            Vector2 origin = new Vector2(tex.Width * 0.5f, (src?.Height ?? tex.Height) * 0.5f);

            // ======================================================================
            // 选一种方式（默认开启“柔和外发光”），二选一即可：
            // ======================================================================

            // 【方式B：描边外轮廓 / Outline】
            // 八方向微偏移描边，再画一次本体，形成实边效果（不切换混合模式）
            bool useOutline = true;
            if (useOutline)
            {
                float scale = 0.90f;
                // 八方向偏移像素（根据缩放取 1~2 像素）
                int px = 2;
                Vector2[] offs =
                {
                    new Vector2(-px, 0), new Vector2(px, 0),
                    new Vector2(0, -px), new Vector2(0, px),
                    new Vector2(-px, -px), new Vector2(px, -px),
                    new Vector2(-px, px), new Vector2(px, px),
                };
                // 描边
                for (int i = 0; i < offs.Length; i++)
                    Main.spriteBatch.Draw(tex, pos + offs[i], src, glowColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
                // 本体
                Main.spriteBatch.Draw(tex, pos, src, Color.White, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }
            else
            {
                // 仅画本体（不发光不描边）
                Main.spriteBatch.Draw(tex, pos, src, Color.White, Projectile.rotation, origin, 0.90f, SpriteEffects.None, 0f);
            }

            return false; // 我们自己完成绘制
        }

    }
}