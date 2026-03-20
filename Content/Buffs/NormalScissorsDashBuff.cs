using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using WuDao.Content.Items.Weapons.Melee;

namespace WuDao.Content.Buffs
{
    // 御剪冲刺buff
    // TODO: 向后冲刺时没有先转身再冲刺 
    public class NormalScissorsDashBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> NormalScissorsTexture;
        public override void SetStaticDefaults()
        {
            // 主动显示、可见倒计时
            Main.debuff[Type] = true;          // 视为debuff（只影响显示分类）
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = true;      // 不存档
            Main.buffNoTimeDisplay[Type] = false; // 显示时间
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;

            NormalScissorsTexture = Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Melee/NormalScissors");
        }
        public override void Unload()
        {
            NormalScissorsTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            // 用 Buff 的剩余时间反向同步到 ModPlayer，让逻辑仍能用 dashCooldown
            var mp = player.GetModPlayer<NormalScissorsPlayer>();
            mp.dashCooldown = player.buffTime[buffIndex];
        }
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            if (Main.dedServ || NormalScissorsTexture == null)
                return;

            Texture2D texture = NormalScissorsTexture.Value;

            Rectangle source = new Rectangle(0, 0, 32, 32);
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