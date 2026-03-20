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
    // 断裂英雄剑 Summon Buff
    public class HeroBrokenSwordBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> HeroBrokenSwordTexture;
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;       // 不存档
            Main.buffNoTimeDisplay[Type] = true;// 不显示计时
        }
        public override void Load()
        {
            // 预加载仅在客户端分支执行
            if (Main.dedServ)
                return;

            Main.instance.LoadItem(ItemID.BrokenHeroSword);
            HeroBrokenSwordTexture = TextureAssets.Item[ItemID.BrokenHeroSword];
        }
        public override void Unload()
        {
            HeroBrokenSwordTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            // 有小兵则刷新时间；没有则移除（与示例一致）
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Summon.HeroBrokenSwordMinion>()] > 0)
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
            if (Main.dedServ || HeroBrokenSwordTexture == null)
                return;
                
            Texture2D texture = HeroBrokenSwordTexture.Value;

            Rectangle sourceRect = new Rectangle(
                0,
                0,
                34,
                38
            );
            
            Vector2 origin = new Vector2(-3, 0);
            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                sourceRect,
                drawParams.DrawColor,
                0,
                origin,
                0.82f,
                SpriteEffects.None,
                0
            );
        }
    }
}
