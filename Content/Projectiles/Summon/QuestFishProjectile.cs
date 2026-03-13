using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Summon;

public class QuestFishProjectile : ModProjectile
{

    public override string Texture => "Terraria/Images/Item_" + ItemID.None;

    public override void SetDefaults()
    {
        Projectile.width = 26;
        Projectile.height = 26;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Summon;

        Projectile.timeLeft = 220;

        Projectile.alpha = 255;
        Projectile.penetrate = -1;
    }

    public override void OnSpawn(IEntitySource source)
    {
        // 水花粒子
        for (int i = 0; i < 6; i++)
        {
            Dust.NewDust(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.Water
            );
        }
    }

    public override void AI()
    {
        // 前半段略微保速，后半段逐步下坠
        if (Projectile.ai[1] < 14f)
        {
            Projectile.ai[1]++;
            Projectile.velocity.Y *= 0.98f;
        }
        else
        {
            Projectile.velocity.Y += 0.28f;
        }

        // 少量水平阻尼
        Projectile.velocity.X *= 0.995f;

        // 淡入淡出
        if (Projectile.timeLeft > 200)
            Projectile.alpha = Math.Max(0, Projectile.alpha - 25);
        else if (Projectile.timeLeft < 24)
            Projectile.alpha = Math.Min(255, Projectile.alpha + 12);

        Projectile.rotation = Projectile.velocity.ToRotation();

        // 如果你的鱼原贴图朝右，这个 rotation 就天然正确
    }
    public override bool PreDraw(ref Color lightColor)
    {
        // 确保已加载
        Texture2D tex = ModContent.Request<Texture2D>("Terraria/Images/Item_" + (int)Projectile.ai[0]).Value;

        // 如果往左边飞，需要垂直翻转贴图
        SpriteEffects effect =
            Projectile.velocity.X < 0
            ? SpriteEffects.FlipVertically
            : SpriteEffects.None;

        Main.EntitySpriteDraw(
            tex,
            Projectile.Center - Main.screenPosition,
            null,
            lightColor * Projectile.Opacity,
            Projectile.rotation,
            tex.Size() / 2,
            1,
            effect,
            0
        );

        return false;
    }
}