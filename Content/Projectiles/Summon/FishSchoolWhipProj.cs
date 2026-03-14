using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;

namespace WuDao.Content.Projectiles.Summon;

public class FishSchoolWhipProj : ModProjectile
{
    public override string Texture => "WuDao/Content/Projectiles/Summon/FlyingSnakeWhipProjectile";

    // 任务鱼生成间隔（同一个目标）
    private const int FishSpawnCooldown = 24; // 24 tick = 0.4 秒
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.IsAWhip[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.DefaultToWhip();
        Projectile.WhipSettings.Segments = 20;
        Projectile.WhipSettings.RangeMultiplier = 1.2f;
    }
    private void DrawLine(List<Vector2> list)
    {
        Texture2D texture = TextureAssets.FishingLine.Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = new Vector2(frame.Width / 2, 2);

        Vector2 pos = list[0];
        for (int i = 0; i < list.Count - 1; i++)
        {
            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;

            float rotation = diff.ToRotation() - MathHelper.PiOver2;
            Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
            Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

            pos += diff;
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        List<Vector2> list = new();
        Projectile.FillWhipControlPoints(Projectile, list);

        DrawLine(list); // 仍然用 FishingLine 画连线

        SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        Vector2 pos = list[0];

        for (int i = 0; i < list.Count - 1; i++)
        {
            // ——对应你 10×92 贴图的默认切片
            Rectangle frame = new Rectangle(0, 0, 10, 26); // 手柄
            Vector2 origin = new Vector2(5, 8);            // 手握点（你可按贴图微调）
            float scale = 1f;

            if (i == list.Count - 2)
            {
                // 鞭梢（10×18，从 Y=74 开始）
                frame.Y = 74;
                frame.Height = 18;

                // 鞭梢伸展时放大（可选，但很像原版/示例）
                Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out _, out _);
                float t = Projectile.ai[0] / timeToFlyOut; // Timer 就是 ai[0]
                scale = MathHelper.Lerp(
                    0.5f, 1.5f,
                    Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true)
                );
            }
            else if (i > 10)
            {
                frame.Y = 58; frame.Height = 16; // 段3
            }
            else if (i > 5)
            {
                frame.Y = 42; frame.Height = 16; // 段2
            }
            else if (i > 0)
            {
                frame.Y = 26; frame.Height = 16; // 段1
            }

            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;
            float rotation = diff.ToRotation() - MathHelper.PiOver2; // 贴图朝下
            Color color = Lighting.GetColor(element.ToTileCoordinates());

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

            pos += diff;
        }

        return false; // 我们自己画完了
    }
    // 目前冷却时对武器而言的，武器打A后马上打B也会被冷却住
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // ai[0] 作为该鞭子对每个目标的本地命中冷却计时入口不合适，
        // 最简单的方法：直接使用 target 的 immunity 概念不太适配这个需求，
        // 所以推荐配一个 GlobalNPC 存 timer。
        // 如果你想先快速实现，这里用 localAI[0] 做“当前鞭子实例的额外射弹冷却”也行。

        if (Projectile.localAI[0] > 0f)
            return;

        Projectile.localAI[0] = FishSpawnCooldown;

        Player player = Main.player[Projectile.owner];

        // int fishCount = Main.rand.Next(2, 5);

        // for (int i = 0; i < fishCount; i++)
        {
            bool spawnOnLeft = Main.rand.NextBool();

            // 以“正下方”为基准，偏移 30°~60°
            float offsetDeg = Main.rand.NextFloat(30f, 60f);
            float offsetRad = MathHelper.ToRadians(offsetDeg);

            // 出生半径
            float radius = Main.rand.NextFloat(90f, 180f);

            Vector2 spawnOffset;

            if (spawnOnLeft)
            {
                // 敌怪正下方往左偏 30~60°
                spawnOffset = Vector2.UnitY.RotatedBy(-offsetRad) * radius;
            }
            else
            {
                // 敌怪正下方往右偏 30~60°
                spawnOffset = Vector2.UnitY.RotatedBy(offsetRad) * radius;
            }

            Vector2 spawnPos = target.Center + spawnOffset;

            Vector2 launchVelocity = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.Next(9, 14);

            int fishItem = ItemSets.TaskFishSet.Get(SelectionMode.Random);

            // 生成任务鱼
            int proj = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos,
                launchVelocity,
                ModContent.ProjectileType<QuestFishProjectile>(),
                (int)(damageDone * 0.8f),
                0f,
                player.whoAmI,
                fishItem
            );
        }
    }

    public override void AI()
    {
        if (Projectile.localAI[0] > 0f)
            Projectile.localAI[0]--;
    }
}