using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Buffs
{
    // 怪物火车 Summon Buff
    public class MonsterTrainBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> MonsterTrainTexture;
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
            
            MonsterTrainTexture = Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Summon/MonsterTrainStaff");
        }
        public override void Unload()
        {
            MonsterTrainTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<MonsterTrainMinion>()] > 0)
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
            if (Main.dedServ || MonsterTrainTexture == null)
                return;
            
            Texture2D texture = MonsterTrainTexture.Value;

            Rectangle sourceRect = new Rectangle(
                5,
                0,
                32,
                32
            );

            Vector2 origin = new Vector2(0, 0);
            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                sourceRect,
                drawParams.DrawColor,
                0,
                origin,
                1f,
                SpriteEffects.None,
                0
            );
        }
    }
}