using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    // 永生之酒冷却buff
    public class WineCooldownBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> WineCooldownTexture;
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;// 作为“负面”显示（仅用于提示，防止被部分清 Buff 手段移除）
            Main.buffNoTimeDisplay[Type] = false;
            Main.pvpBuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;

            WineCooldownTexture = Mod.Assets.Request<Texture2D>("Content/Items/EverlastingWine");
        }
        public override void Unload()
        {
            WineCooldownTexture = null;
        }
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            if (Main.dedServ || WineCooldownTexture == null)
            {
                return;
            }
            Texture2D texture = WineCooldownTexture.Value;

            Rectangle source = new Rectangle(0, 0, 32, 32);

            Vector2 origin = new Vector2(0, 0);

            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                source,
                drawParams.DrawColor, // 不要再用 Color.White
                0f,
                origin,
                1,
                SpriteEffects.None,
                0
            );
        }
    }
}
