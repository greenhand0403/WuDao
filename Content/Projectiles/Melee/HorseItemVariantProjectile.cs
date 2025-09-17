using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using System;

namespace WuDao.Content.Projectiles.Melee
{
    /// <summary>
    /// 复用三张物品贴图（ItemID=4785, 4786, 4787）来绘制一个“骏马形”的射弹。
    /// 通过生成时传入 ai0 选择贴图：0->4785，1->4786，2->4787。
    /// </summary>
    public class HorseItemVariantProjectile : ModProjectile
    {
        private static readonly int[] VariantItemIds = new int[] { 4785, 4786, 4787 };
        public override string Texture => "Terraria/Images/Projectile_0"; // 隐藏占位，使用 PreDraw 绘制物品贴图

        public override void SetDefaults()
        {
            Projectile.width = 64;   // 粗略碰撞箱，必要时可在 OnSpawn 根据贴图重新设定
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = 0;
            Projectile.hide = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 设定朝向
            Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X >= 0 ? 1 : -1;
        }

        public override void AI()
        {
            // 基础直线飞行 + 旋转对齐速度
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
        }

        private int GetSelectedItemId()
        {
            int idx = (int)MathHelper.Clamp((float)Math.Round(Projectile.ai[0]), 0f, VariantItemIds.Length - 1);
            return VariantItemIds[idx];
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int itemId = GetSelectedItemId();
            Texture2D tex = TextureAssets.Item[itemId].Value;

            // 以贴图中心为原点绘制
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 让光照影响颜色
            Color drawColor = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));

            Main.EntitySpriteDraw(tex, drawPos, null, drawColor, Projectile.rotation, origin, 1f, fx, 0);
            return false;
        }

        // 可选：与地形碰撞后消失
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }
    }
}