using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 法外狂徒霰弹枪 后跳冷却buff
    public class OutlawBackstepBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> OutlawTexture;
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;               // 用 debuff 风格显示
            Main.pvpBuff[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;   // 显示倒计时
            Main.buffNoSave[Type] = true;           // 不存档
        }
        public override void Load()
        {
            OutlawTexture = Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Ranged/TheOutlaw");
        }
        public override void Unload()
        {
            OutlawTexture = null;
        }
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            if (Main.dedServ || OutlawTexture == null)
                return;

            Texture2D texture = OutlawTexture.Value;

            Rectangle source = new Rectangle(0, 0, 32, 32);
            Vector2 origin = new Vector2(-4, -8);

            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                source,
                drawParams.DrawColor, // 不要再用 Color.White
                0f,
                origin,
                0.8f,
                SpriteEffects.None,
                0
            );
        }
    }
}
