using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Mounts;

namespace WuDao.Content.Buffs
{
    // 飞天马桶坐骑buff
    public class FlyingToiletBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; // 坐骑 buff 无限
            Main.buffNoSave[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            // 每 tick 重置 buffTime
            player.buffTime[buffIndex] = 10;

            if (player.mount.Type != ModContent.MountType<FlyingToiletMount>())
            {
                player.mount.SetMount(ModContent.MountType<FlyingToiletMount>(), player);
            }
        }
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            // 确保已加载
            Texture2D texture = ModContent.Request<Texture2D>("Terraria/Images/Item_" + ItemID.TerraToilet).Value;

            Rectangle source = new Rectangle(0, 0, 16, 30);
            // 32-16 = 16, 16 / 2 = 8
            Vector2 origin = new Vector2(-8, 0);

            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                source,
                drawParams.DrawColor, // 不要再用 Color.White
                0f,
                origin,
                1f,
                SpriteEffects.None,
                0
            );
        }
    }
}
