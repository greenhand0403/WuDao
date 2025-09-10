using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Throwing
{
    public class FoodRainProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.CookedFish; // 占位，不会用这张

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = false;
            Projectile.hostile = true;           // 默认按“红弹”处理；绿弹会在 AI 里关闭
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
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

        public override bool PreDraw(ref Color lightColor)
        {
            // 直接画物品贴图：Main.itemTexture[itemId]
            var tex = Terraria.GameContent.TextureAssets.Item[ItemType].Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            // 先画发光外圈（简化：缩放的同色圈）
            Texture2D circle = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            float r = 20f;
            var c = IsRed ? new Color(255, 80, 80, 90) : new Color(80, 255, 80, 90);
            Main.spriteBatch.Draw(circle, new Rectangle((int)pos.X - (int)r, (int)pos.Y - (int)r, (int)(r * 2), (int)(r * 2)), c);

            Main.spriteBatch.Draw(tex, pos, null, Color.White, Projectile.rotation, origin, 0.8f, SpriteEffects.None, 0f);
            return false;
        }
    }
}