using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using WuDao.Content.Projectiles;

namespace WuDao.Content.Buffs
{
    // 丘比跟随宠物Buff：显示为虚饰宠物，维持时间并保证召唤
    public class KyubeyPetBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; // 不显示计时
            Main.vanityPet[Type] = true;         // 虚饰/宠物 Buff
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            // 如果需要则生成宠物，并将 buff 时间维持在 2 tick（持续刷新）
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused,
                ModContent.ProjectileType<KyubeyPetProjectile>());
        }
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            Texture2D texture = ModContent.Request<Texture2D>("WuDao/Content/Projectiles/KyubeyPetProjectile").Value;

            Rectangle sourceRect = new Rectangle(
                34,
                0,
                32,
                32
            );
            // 绘制时往右偏移一点点
            Vector2 origin = new Vector2(-4, 0);
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