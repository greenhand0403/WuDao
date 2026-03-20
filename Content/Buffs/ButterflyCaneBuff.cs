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
    // 蝴蝶杖 Summon Buff
    public class ButterflyCaneBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> ButterflyItemTexture;
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

            Main.instance.LoadItem(ItemID.TreeNymphButterfly);
            ButterflyItemTexture = TextureAssets.Item[ItemID.TreeNymphButterfly];
        }

        public override void Unload()
        {
            ButterflyItemTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ButterflyMinion>()] > 0)
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
            if (Main.dedServ || ButterflyItemTexture == null)
                return;

            Texture2D texture = ButterflyItemTexture.Value;

            Rectangle source = new Rectangle(0, 0, 32, 32);
            
            Vector2 origin = new Vector2(-2, -2);

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