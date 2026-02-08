using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Tiles;

namespace WuDao.Content.Systems
{
    // 在地下层生成许愿井
    public class WishingWellWorldGenSystem : ModSystem
    {
        public override void PostWorldGen()
        {
            // 只在服务端/单机执行；世界生成阶段通常是 netMode == 0
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int wellTileType = ModContent.TileType<WishingWellTile>();

            int centerX = Main.maxTilesX / 2;

            // 从地表附近开始找（避免空中/海底）
            int yStart = (int)Main.worldSurface - 50;
            int yEnd = (int)Main.worldSurface + 250;
            yStart = Utils.Clamp(yStart, 50, Main.maxTilesY - 300);
            yEnd = Utils.Clamp(yEnd, yStart + 50, Main.maxTilesY - 300);

            // 左右扩展搜索半径（按需调大）
            int searchRadiusX = 300;

            for (int dx = 0; dx <= searchRadiusX; dx++)
            {
                // 先右后左（或你喜欢的顺序）
                if (TryPlaceWellAtX(centerX + dx, yStart, yEnd, wellTileType))
                    return;

                if (dx != 0 && TryPlaceWellAtX(centerX - dx, yStart, yEnd, wellTileType))
                    return;
            }

            // 找不到就放弃（也可以改成再扩大范围/改搜索高度）
        }

        private bool TryPlaceWellAtX(int x, int yStart, int yEnd, int wellTileType)
        {
            // 需要 3 格宽：x-1, x, x+1 都要在世界内
            if (x - 1 < 5 || x + 1 >= Main.maxTilesX - 5)
                return false;

            for (int y = yStart; y <= yEnd; y++)
            {
                // 找“地面”：这一行是实心方块，并且 3 格都实心，作为承重
                if (!IsSolid(x - 1, y) || !IsSolid(x, y) || !IsSolid(x + 1, y))
                    continue;

                // 物件 Origin(1,2)，3×3：如果承重在 y，那么 originY 取 y-1（物件坐在地面上）
                int originX = x;
                int originY = y - 1;

                // 物件占用区域的左上角（3×3）
                int left = originX - 1;
                int top = originY - 2;

                // 检查 3×3 空间必须是空的（不能有砖/墙上其他物件占格）
                if (!AreaIsClear(left, top, 3, 3))
                    continue;

                // 可选：不要放在岩浆/蜂蜜里
                if (HasBadLiquid(left, top, 3, 3))
                    continue;

                // 真正放置：PlaceObject 的坐标是“Origin 点”！
                bool placed = WorldGen.PlaceObject(originX, originY, wellTileType, mute: true, style: 0);
                if (!placed)
                    continue;

                // 放置 TileEntity（你的 WishingWellTile.PlaceInWorld 在 worldgen 时未必会触发，手动补最稳）
                // TE 的坐标用“左上角”
                ModContent.GetInstance<WishingWellTE>().Place(left, top);

                // 刷新区块
                WorldGen.SquareTileFrame(originX, originY, true);
                return true;
            }

            return false;
        }

        private static bool IsSolid(int x, int y)
        {
            if (!WorldGen.InWorld(x, y, 10))
                return false;

            var t = Framing.GetTileSafely(x, y);
            // SolidTile 会考虑 active + solid 等情况，比自己判断更省心
            return WorldGen.SolidTile(t);
        }

        private static bool AreaIsClear(int left, int top, int width, int height)
        {
            for (int x = left; x < left + width; x++)
            {
                for (int y = top; y < top + height; y++)
                {
                    if (!WorldGen.InWorld(x, y, 10))
                        return false;

                    Tile t = Framing.GetTileSafely(x, y);

                    // 有“占格的砖/物件”就不行
                    if (t.HasTile)
                        return false;
                }
            }
            return true;
        }

        private static bool HasBadLiquid(int left, int top, int width, int height)
        {
            for (int x = left; x < left + width; x++)
            {
                for (int y = top; y < top + height; y++)
                {
                    Tile t = Framing.GetTileSafely(x, y);
                    if (t.LiquidAmount > 0)
                    {
                        // lava/honey/shimmer 都算不适合（你也可以只排除 lava）
                        if (t.LiquidType == LiquidID.Lava ||
                            t.LiquidType == LiquidID.Honey ||
                            t.LiquidType == LiquidID.Shimmer ||
                            t.LiquidAmount > 0)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
