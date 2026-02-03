using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WuDao.Content.Systems;
using System;

namespace WuDao.Content.Players
{
    // TODO: 在出生点下方生成许愿井
    public class WudaoPlantAnnouncer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            // var sys = ModContent.GetInstance<WishingWellSystem>();
            // if (sys.GetPrintedOnce()) return;

            // var ws = sys.GetWishingWellPositions();

            // foreach (var p in ws)
            //     Main.NewText("  - " + FormatTilePosHumanReadable(p.X, p.Y), Color.MediumPurple);

            // sys.SetPrintedOnce(); // 标记已打印
        }
        private static string FormatTilePosHumanReadable(int x, int y)
        {
            int center = Main.maxTilesX / 2;
            string ew = x < center ? "西" : (x > center ? "东" : "世界中心");
            int dx = Math.Abs(x - center);

            int surfaceY = (int)Main.worldSurface;
            int dyTiles = y - surfaceY;           // + = 地表以下, - = 地表以上
            int dyFeet = dyTiles * 2;
            string ud = dyTiles >= 0 ? "地表以下" : "地表以上";

            return $"({x}, {y}) — {ew}{dx}格，{ud}{Math.Abs(dyFeet)}英尺";
        }
    }
}
