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
    // 附魔狼牙项链召唤buff
    public class GreedyWolfBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> GreedyWolfTexture;
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

            Main.instance.LoadNPC(NPCID.Wolf);
            GreedyWolfTexture = TextureAssets.Npc[NPCID.Wolf];
        }

        public override void Unload()
        {
            GreedyWolfTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<GreedyWolfMinion>()] > 0)
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
            if (Main.dedServ || GreedyWolfTexture == null)
                return;

            Texture2D texture = GreedyWolfTexture.Value;

            Rectangle source = new Rectangle(54, 2, 30, 30);
            // 使绘制狼图案时往右边挪2个像素，往上挪是+1个像素
            Vector2 origin = new Vector2(-2, 0);

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
