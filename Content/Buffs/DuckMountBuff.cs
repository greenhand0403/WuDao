using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    public class DuckMountBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> DuckMountTexture;
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Load()
        {
            if (Main.dedServ)
            {
                return;
            }

            Main.instance.LoadNPC(NPCID.Duck);
            DuckMountTexture = TextureAssets.Npc[NPCID.Duck];
        }
        public override void Unload()
        {
            DuckMountTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.mount.Type != ModContent.MountType<Mounts.DuckMount>())
            {
                player.mount.SetMount(ModContent.MountType<Mounts.DuckMount>(), player);
            }

            player.buffTime[buffIndex] = 10;
        }
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            // 确保已加载纹理
            if (Main.dedServ || DuckMountTexture == null)
                return;

            Texture2D texture = DuckMountTexture.Value;

            Rectangle source = new Rectangle(0, 0, 52, 52);
            
            Vector2 origin = new Vector2(-2, -2);

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