using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Systems
{
    public class WindBladeWorldGenSystem : ModSystem
    {
        public override void PostWorldGen()
        {
            int windBladeType = ModContent.ItemType<Content.Items.Weapons.Magic.WindBlade>();
            const int SkywareChestStyle = 11; // 经典 Skyware 样式编号

            // 遍历全地图的 Chest
            for (int i = 0; i < Main.maxChests; i++)
            {
                Chest chest = Main.chest[i];
                if (chest == null) continue;

                // 找到该 Chest 在地图上的 tile（Containers）
                int x = chest.x;
                int y = chest.y;

                Tile tile = Framing.GetTileSafely(x, y);
                if (tile == null) continue;

                // 只处理容器（宝箱）
                if (tile.TileType != TileID.Containers) continue;

                // 计算样式：每 36px 一格 frameX
                int style = tile.TileFrameX / 36;
                if (style != SkywareChestStyle) continue;

                // 1/3 概率加入 WindBlade（不会替换原有掉落，只是“额外可能出现”）
                if (WorldGen.genRand.NextBool(3))
                {
                    // 找到一个空位
                    for (int slot = 0; slot < Chest.maxItems; slot++)
                    {
                        if (chest.item[slot].type == ItemID.None || chest.item[slot].stack <= 0)
                        {
                            chest.item[slot].SetDefaults(windBladeType);
                            chest.item[slot].stack = 1;
                            break;
                        }
                    }
                }
            }
        }
    }
}
