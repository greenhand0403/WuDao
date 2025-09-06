using Microsoft.Xna.Framework;
using Terraria;

namespace WuDao.Common
{
    public static class Helpers
    {
        public static Vector2 MouseWorld() => Main.MouseWorld;

        public static bool IsPlayerAttackingOrMoving(Player p)
        {
            // 站立或不动：不按方向键 & itemAnimation == 0
            bool moving = p.controlLeft || p.controlRight || p.controlUp || p.controlDown || p.velocity.LengthSquared() > 0.01f;
            bool attacking = p.itemAnimation > 0;
            return moving || attacking;
        }

        public static Rectangle ScreenBoundsWorldSpace()
        {
            return new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
        }
    }
}
