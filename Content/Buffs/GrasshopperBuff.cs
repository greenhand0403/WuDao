using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Buffs
{
    // 蚱蜢 Summon Buff
    public class GrasshopperBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> GrasshopperTexture;
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Load()
        {
            // 预加载仅在客户端分支执行
            if (Main.dedServ)
                return;
            
            Main.instance.LoadNPC(NPCID.Grasshopper);
            GrasshopperTexture = TextureAssets.Npc[NPCID.Grasshopper];
        }
        public override void Unload()
        {
            GrasshopperTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<GrasshopperMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }

        // 在默认buff底图的基础上绘制蚱蜢头像，组合成buff图标
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            if (Main.dedServ||GrasshopperTexture == null)
            {
                return;
            }
            
            Texture2D texture = GrasshopperTexture.Value;

            Rectangle sourceRect = new Rectangle(
                0,
                0,
                18,
                12
            );
            Vector2 origin = new Vector2(-2, -4);
            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                sourceRect,
                drawParams.DrawColor,
                0,
                origin,
                1.8f,
                SpriteEffects.None,
                0
            );
        }
    }
}