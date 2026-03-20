using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.DrawLayers
{
    public class SheRaGlowLayer : PlayerDrawLayer
    {
        private static Asset<Texture2D> headGlow;
        private static Asset<Texture2D> bodyGlow;
        private static Asset<Texture2D> legsGlow;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            headGlow = Mod.Assets.Request<Texture2D>("Content/Items/Armor/SheRaSword_Head_Glow");
            bodyGlow = Mod.Assets.Request<Texture2D>("Content/Items/Armor/SheRaSword_Body_Glow");
            legsGlow = Mod.Assets.Request<Texture2D>("Content/Items/Armor/SheRaSword_Legs_Glow");
        }
        public override void Unload()
        {
            headGlow = null;
            bodyGlow = null;
            legsGlow = null;
        }
        public override Position GetDefaultPosition()
        {
            return new Between(
                PlayerDrawLayers.Head,
                PlayerDrawLayers.ArmOverItem
            );
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            return player.GetModPlayer<SheRaSwordPlayer>().IsTransformed;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<SheRaSwordPlayer>();

            if (!modPlayer.IsTransformed)
                return;

            // 当前人物帧
            Rectangle bodyFrame = player.bodyFrame;
            Rectangle legFrame = player.legFrame;

            Vector2 position = drawInfo.Center - Main.screenPosition;

            Vector2 bodyPos = new Vector2(
                (int)(position.X + player.width * 0f),
                (int)(position.Y + player.height * 0f)
            );
            // 特判，如果有坐骑，则需要增加坐骑的偏移量
            if (player.mount.Active)
            {
                bodyPos.Y += player.mount.YOffset;
            }

            SpriteEffects effects = drawInfo.playerEffect;

            // 头部 Glow
            DrawData headData = new DrawData(
                headGlow.Value,
                bodyPos + player.headPosition + new Vector2(0f, player.gfxOffY - 4f),
                bodyFrame,
                Color.White,
                player.headRotation,
                bodyFrame.Size() / 2f,
                1f,
                effects,
                0
            );
            drawInfo.DrawDataCache.Add(headData);

            // 身体 Glow
            DrawData bodyData = new DrawData(
                bodyGlow.Value,
                bodyPos + player.bodyPosition + new Vector2(0f, player.gfxOffY),
                bodyFrame,
                Color.White,
                player.bodyRotation,
                bodyFrame.Size() / 2f,
                1f,
                effects,
                0
            );
            drawInfo.DrawDataCache.Add(bodyData);

            // 腿部 Glow
            DrawData legsData = new DrawData(
                legsGlow.Value,
                bodyPos + player.legPosition + new Vector2(0f, player.gfxOffY),
                legFrame,
                Color.White,
                player.legRotation,
                legFrame.Size() / 2f,
                1f,
                effects,
                0
            );
            drawInfo.DrawDataCache.Add(legsData);
        }
    }
}