using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Buffs
{
    public class SheRaTransformBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> SheRaSwordTexture;
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;

            SheRaSwordTexture = Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Melee/SheRaSword");
        }
        public override void Unload()
        {
            SheRaSwordTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            var modPlayer = player.GetModPlayer<SheRaSwordPlayer>();

            // 如果变身已经结束，移除 Buff
            if (!modPlayer.IsTransformed)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            if (Main.dedServ || SheRaSwordTexture == null)
            {
                return;
            }
            Texture2D texture = SheRaSwordTexture.Value;

            Rectangle source = new Rectangle(0, 0, 52, 52);

            Vector2 origin = new Vector2(0, 0);

            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                source,
                drawParams.DrawColor, // 不要再用 Color.White
                0f,
                origin,
                0.6f,
                SpriteEffects.None,
                0
            );
        }
    }
}