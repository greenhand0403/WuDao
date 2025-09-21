using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using WuDao.Common;

namespace WuDao.Content.Projectiles.Throwing
{
    // 宝石射弹：继承基类，使用不同的贴图并在击中时加一些光效
    public class GemProjectile : BaseThrowingProjectile
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Diamond}";
        // 允许通过外部设置贴图索引（在 Spawn 前通过 proj.frame 或者在构造时用 type 指定贴图）
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 420;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.5f;
        }

        public override void ImpactEffects(Vector2 position, Vector2 velocity)
        {
            // 用宝石风格的尘粒
            for (int i = 0; i < 12; i++)
            {
                int d = Dust.NewDust(position, Projectile.width, Projectile.height, DustID.GemRuby + Main.rand.Next(0, 7));
                Main.dust[d].velocity *= 0.6f;
                Main.dust[d].noGravity = true;
            }
        }

        // 选择性地画不同帧（如果你提供了多帧贴图）
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Item[ItemSets.GemSet.Get(SelectionMode.Random)].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}