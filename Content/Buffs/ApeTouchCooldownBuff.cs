using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    public class ApeTouchCooldownBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> ApeTouchCooldownTexture;
        public override void Load()
        {
            // 预加载仅在客户端分支执行
            if (Main.dedServ)
                return;

            ApeTouchCooldownTexture = Mod.Assets.Request<Texture2D>("Content/Items/Accessories/ApeTouch");
        }
        public override void Unload()
        {
            ApeTouchCooldownTexture = null;
        }
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
        }
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            if (Main.dedServ || ApeTouchCooldownTexture == null)
                return;

            Texture2D texture = ApeTouchCooldownTexture.Value;

            Rectangle sourceRect = new Rectangle(
                0,
                0,
                48,
                48
            );
            // 绘制时往右偏移一点点
            Vector2 origin = new Vector2(0, 0);
            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                sourceRect,
                drawParams.DrawColor,
                0,
                origin,
                0.65f,
                SpriteEffects.None,
                0
            );
        }
    }
}