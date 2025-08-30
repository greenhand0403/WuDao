
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Enums;
using Terraria.ID;
using Terraria.DataStructures;
using WuDao.Content.Buffs;

namespace WuDao.Content.Tiles
{
    /// <summary>
    /// 3x2 雕像方块，向周围玩家赋予“金字塔守护”Buff（极短时长反复刷新）。
    /// 具体数值在 BuffPlayer 中处理。
    /// </summary>
    public class PyramidStatueTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileSolidTop[Type] = false;
            Main.tileSolid[Type] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Origin = new Point16(1, 1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(237, 206, 125), CreateMapEntryName());
            DustType = DustID.Sand;
            HitSound = SoundID.Dig;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            const int range = 16 * 20;
            // 小范围光环：对半径 ~320 像素（约 20 格）内玩家赋予短 Buff 1 格 16像素
            Rectangle rect = new Rectangle(i * 16 - range, j * 16 - range, 16 * 3 + range, 16 * 2 + range);
            for (int idx = 0; idx < Main.maxPlayers; idx++)
            {
                Player p = Main.player[idx];
                if (p != null && p.active && p.getRect().Intersects(rect))
                {
                    int ib = p.FindBuffIndex(ModContent.BuffType<PyramidWard>());
                    if (ib >= 0)
                    {
                        // 只有在剩余时间偏低时才回填，避免每帧重置导致的闪烁
                        if (p.buffTime[ib] < 60)
                            p.buffTime[ib] = 180;
                    }
                    else
                    {
                        p.AddBuff(ModContent.BuffType<PyramidWard>(), 180); // 持续刷新
                    }
                }
            }
        }
    }
}
