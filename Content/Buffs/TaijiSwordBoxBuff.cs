using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Buffs
{
    // 太极剑匣 Summon Buff
    public class TaijiSwordBoxBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> TaijiSwordBoxTexture;
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;

            TaijiSwordBoxTexture = Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Summon/TaijiSwordBox");
        }
        public override void Unload()
        {
            TaijiSwordBoxTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<TaijiSwordMinion>()] > 0)
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
            if (Main.dedServ || TaijiSwordBoxTexture == null)
            {
                return;
            }
            Texture2D texture = TaijiSwordBoxTexture.Value;

            Rectangle source = new Rectangle(0, 0, 32, 32);

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