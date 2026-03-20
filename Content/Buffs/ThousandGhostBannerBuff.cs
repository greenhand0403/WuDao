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
    // 万魂幡 Summon Buff
    public class ThousandGhostBannerBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> ThousandGhostBannerTexture;
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; // 这个 buff 不显示持续时间
            Main.buffNoSave[Type] = true; // 这个 buff 不保存
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;
                
            Main.instance.LoadNPC(NPCID.Ghost);
            ThousandGhostBannerTexture = TextureAssets.Npc[NPCID.Ghost];
        }
        public override void Unload()
        {
            ThousandGhostBannerTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ThousandGhostMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            if (Main.dedServ || ThousandGhostBannerTexture == null)
            {
                return;
            }
            Texture2D texture = ThousandGhostBannerTexture.Value;

            Rectangle source = new Rectangle(0, 0, 40, 40);

            Vector2 origin = new Vector2(0, 0);

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