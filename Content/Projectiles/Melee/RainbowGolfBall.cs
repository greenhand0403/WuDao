using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles
{
    // 近战版“彩虹高尔夫球”，克隆原版高尔夫球的物理与 AI
    public class RainbowGolfBall : ModProjectile
    {
        // 使用原版高尔夫球贴图（白球）
        public override string Texture => TextureAssets.Projectile[ProjectileID.DirtGolfBall].Name;

        public override void SetDefaults()
        {
            // 克隆原版高尔夫球的物理/AI/碰撞等
            Projectile.CloneDefaults(ProjectileID.DirtGolfBall);

            // 调整为友方近战伤害
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;

            // 可选：让本弹体不受弹药/武器射速加成之外的多余修改
            // Projectile.penetrate、width/height、aiStyle 等均已随 CloneDefaults 同步
        }

        // ai[0] 存储我们循环到的“颜色索引”（0..15），用于上色显示（不改变物理）
        private static readonly Color[] _tints = new Color[] {
            // 16 个代表色：和 4242..4255 一一对应（你也可以换成自己喜欢的配色）
            new Color(255,255,255),  // 4242
            new Color(255,59,59),    // 4243
            new Color(255,128,0),    // 4244
            new Color(255,200,0),    // 4245
            new Color(144,238,144),  // 4246
            new Color(0,200,0),      // 4247
            new Color(64,224,208),   // 4248
            new Color(0,191,255),    // 4249
            new Color(65,105,225),   // 4250
            new Color(106,90,205),   // 4251
            new Color(138,43,226),   // 4252
            new Color(199,21,133),   // 4253
            new Color(255,105,180),  // 4254
            new Color(210,180,140),  // 4255（之后两个可自定）
            new Color(220,220,220),
            new Color(50,50,50)
        };

        public override bool PreDraw(ref Color lightColor)
        {
            // 用轻度上色（叠乘）来“显示”不同颜色，同时保留原版贴图细节
            // 你可以把 alpha 调暗一点避免颜色太冲
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.rotation;
            Vector2 origin = Utils.Size(tex.Bounds) * 0.5f;
            float scale = Projectile.scale;

            int idx = (int)(Projectile.ai[0] % _tints.Length);
            if (idx < 0) idx += _tints.Length;

            // 合成出的颜色：受光照影响 + 目标色微加权
            // 这样在黑暗处仍能看清颜色
            Color tint = _tints[idx];
            // 与环境光混合（权重可调）
            Color final = new Color(
                (byte)MathHelper.Clamp(lightColor.R * 0.6f + tint.R * 0.4f, 0, 255),
                (byte)MathHelper.Clamp(lightColor.G * 0.6f + tint.G * 0.4f, 0, 255),
                (byte)MathHelper.Clamp(lightColor.B * 0.6f + tint.B * 0.4f, 0, 255),
                255
            );

            Main.EntitySpriteDraw(tex, drawPos, null, final, rotation, origin, scale, SpriteEffects.None);
            return false; // 原版默认绘制取消，我们已手动绘制
        }
    }
}
