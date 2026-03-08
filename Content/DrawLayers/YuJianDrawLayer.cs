using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.DrawLayers
{
    public class YuJianDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition()
            => new AfterParent(PlayerDrawLayers.MountBack);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            QiPlayer qi = player.GetModPlayer<QiPlayer>();

            if (!qi.YuJianActive || qi.YuJianSwordType <= 0)
                return;

            Texture2D tex = TextureAssets.Item[qi.YuJianSwordType].Value;
            if (tex == null)
                return;

            // 1. 贴图中心
            Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);

            // 2. 对齐到玩家底部中心
            Vector2 worldPos = player.Bottom;
            Vector2 drawPos = worldPos - Main.screenPosition;

            // 3. 根据贴图宽高，计算“矩形对角线”角度
            //    表示剑默认朝向右上
            float rotation = MathF.Atan2(tex.Height, tex.Width);

            // 4. 玩家朝左时，水平翻转
            SpriteEffects effects = SpriteEffects.None;
            if (player.direction == -1)
            {
                rotation -= MathHelper.PiOver2;
                effects = SpriteEffects.FlipHorizontally;
            }

            // 5. 倒重力时需要把角度反过来
            if (player.gravDir == -1f)
            {
                rotation = -rotation;
            }

            // 6. 让剑稍微“贴脚下”一点，可按观感微调
            //    这里沿重力反方向上抬一点，避免正好穿过脚底中心太生硬
            drawPos.Y -= 4f * player.gravDir;

            drawInfo.DrawDataCache.Add(new DrawData(
                tex,
                drawPos,
                null,
                drawInfo.colorArmorBody,
                rotation,
                origin,
                1f,
                effects,
                0
            ));
        }
    }
}