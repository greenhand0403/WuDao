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
    // 飞蛇杖 Summon Buff
    public class FlyingSnakeBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> FlyingSnakeTexture;
        public override void SetStaticDefaults()
        {
            // 召唤类 Buff 的基本设置
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;

            Main.instance.LoadNPC(NPCID.FlyingSnake);
            FlyingSnakeTexture = TextureAssets.Npc[NPCID.FlyingSnake];
        }
        public override void Unload()
        {
            FlyingSnakeTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<FlyingSnakeMinion>()] > 0)
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
            // 确保已加载纹理
            if (Main.dedServ || FlyingSnakeTexture == null)
                return;

            Texture2D texture = FlyingSnakeTexture.Value;

            Rectangle source = new Rectangle(0, 0, 64, 64);

            Vector2 origin = new Vector2(0, 0);

            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                source,
                drawParams.DrawColor, // 不要再用 Color.White
                0f,
                origin,
                0.5f,
                SpriteEffects.None,
                0
            );
        }
    }
}