using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Projectiles.Summon;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace WuDao.Content.Buffs
{
    public class ZombieMinionBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ZombieMinion>()] > 0)
                player.buffTime[buffIndex] = 18000;
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
        public override bool RightClick(int buffIndex)
        {
            // 允许右键取消
            return true;
        }
        // 在默认buff底图的基础上绘制僵尸头像，组合成buff图标
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            // 绘制僵尸的头像作为buff，从贴图38x48的单帧中裁剪32x32的区域
            Texture2D texture = TextureAssets.Npc[NPCID.Zombie].Value;
            // 贴图坐标是左上角为原点
            Rectangle sourceRect = new Rectangle(
                3,// (38-32)/2 对齐中心
                0,
                32,
                32
            );
            // 往右偏移2像素，负数是往右下角挪
            Vector2 origin = new Vector2(-2, 0);
            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                sourceRect,
                drawParams.DrawColor,
                0,
                origin,
                1,
                SpriteEffects.None,
                0
            );
        }
    }
}