using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items;

namespace WuDao.Content.Systems
{
    // 在丛林箱生成异果和灵芝
    public class JungleIvyChestLootSystem : ModSystem
    {
        // Ivy Chest 的箱子 style（TileID.Containers 的 frameX / 36）
        // 官方 Wiki 记法：Internal Tile ID: 21 (10) => style 10
        private const int IvyChestStyle = 10;

        public override void PostWorldGen()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int passionFruitType = ModContent.ItemType<PassionFruit>(); // 你的异果类名
            int reiShiType = ModContent.ItemType<ReiShi>();       // 你的灵芝类名

            for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
            {
                Chest chest = Main.chest[chestIndex];
                if (chest == null)
                    continue;

                if (!IsIvyChest(chest))
                    continue;

                // 30% 放：1 灵芝
                // 50% 放：1 异果
                if (WorldGen.genRand.NextFloat() <= 0.3f)
                    AddToChestPreferStack(chest, reiShiType, 1);
                if (WorldGen.genRand.NextFloat() <= 0.5f)
                    AddToChestPreferStack(chest, passionFruitType, 1);
            }
        }

        private static bool IsIvyChest(Chest chest)
        {
            // chest.x, chest.y 是箱子 2x2 的左上角（通常如此），但为稳妥我们扫 2x2 任意一格
            for (int dx = 0; dx <= 1; dx++)
                for (int dy = 0; dy <= 1; dy++)
                {
                    int x = chest.x + dx;
                    int y = chest.y + dy;

                    if (!WorldGen.InWorld(x, y, 10))
                        continue;

                    Tile t = Framing.GetTileSafely(x, y);
                    if (!t.HasTile)
                        continue;

                    // 普通箱子一般在 Containers（有些版本/特殊箱子在 Containers2，但 Ivy Chest 是 Containers）
                    if (t.TileType != TileID.Containers)
                        continue;

                    int style = t.TileFrameX / 36;
                    if (style == IvyChestStyle)
                        return true;
                }

            return false;
        }

        private static void AddToChestPreferStack(Chest chest, int itemType, int stack)
        {
            // 1) 先找同类，能堆叠就堆叠（避免占用新格子）
            for (int i = 0; i < Chest.maxItems; i++)
            {
                Item it = chest.item[i] ??= new Item();
                if (!it.IsAir && it.type == itemType)
                {
                    it.stack += stack;
                    return;
                }
            }

            // 2) 找空格塞进去
            for (int i = 0; i < Chest.maxItems; i++)
            {
                Item it = chest.item[i] ??= new Item();
                if (it.IsAir)
                {
                    it.SetDefaults(itemType);
                    it.stack = stack;
                    return;
                }
            }

            // 3) 满了就不动（不覆盖原箱子战利品）
        }
    }
}
