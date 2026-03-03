using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using WuDao.Common;
using WuDao.Content.Config;

namespace WuDao.Content.Global
{
    public class InvisibleEnemiesGlobalTile : GlobalTile
    {
        public override bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
        {
            // 服务器不做视觉处理
            if (Main.dedServ) return true;

            if (InvisibleTileRuntime.IsTileInvisible((ushort)type))
            {
                var viewer = Main.LocalPlayer;
                if (viewer == null) return true; // 安全保险

                if (!InvisibleEnemies.CanSeeEcho(viewer))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
