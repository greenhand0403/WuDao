using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Players;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace WuDao.Content.Buffs
{
    class GelFlaskBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        public override void SetStaticDefaults()
        {
            BuffID.Sets.IsAFlaskBuff[Type] = true;
            Main.meleeBuff[Type] = true;
            Main.persistentBuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<WeaponEnchantmentPlayer>().GelFlaskImbue = true;
            player.MeleeEnchantActive = true; // MeleeEnchantActive indicates to other mods that a weapon imbue is active.
        }
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            // 确保已加载
            Texture2D texture = ModContent.Request<Texture2D>("WuDao/Content/Items/GelFlask").Value;

            Rectangle source = new Rectangle(0, 0, 18, 26);
            // 32-18 = 14, 14 / 2 = 7
            Vector2 origin = new Vector2(-7, 0);

            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                source,
                drawParams.DrawColor, // 不要再用 Color.White
                0f,
                origin,
                1.2f,
                SpriteEffects.None,
                0
            );
        }
    }
}
