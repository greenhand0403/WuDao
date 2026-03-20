using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using WuDao.Content.Mounts;

namespace WuDao.Content.Buffs
{
    // 月亮船坐骑buff
    public class LunarShipBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> LunarShipTexture;
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; // 坐骑 buff 无限
            Main.buffNoSave[Type] = true;
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;

            LunarShipTexture = Mod.Assets.Request<Texture2D>("Content/Mounts/LunarShip_Front");
        }
        public override void Unload()
        {
            LunarShipTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            // 每 tick 重置 buffTime
            player.buffTime[buffIndex] = 10;

            if (player.mount.Type != ModContent.MountType<LunarShip>())
            {
                player.mount.SetMount(ModContent.MountType<LunarShip>(), player);
            }
        }
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            if (Main.dedServ || LunarShipTexture == null)
                return;
                
            Texture2D texture = LunarShipTexture.Value;

            Rectangle sourceRect = new Rectangle(
                7,
                0,
                64,
                64
            );

            Vector2 origin = new Vector2(0, 0);
            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                sourceRect,
                drawParams.DrawColor,
                0,
                origin,
                0.5f,
                SpriteEffects.None,
                0
            );
        }
    }
}
