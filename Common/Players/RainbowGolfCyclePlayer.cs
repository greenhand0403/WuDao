using Terraria;
using Terraria.ModLoader;

namespace WuDao.Common.Players
{
    // 负责在 4242–4255 之间循环索引
    public class RainbowGolfCyclePlayer : ModPlayer
    {
        // 对应物品ID 4242..4255 的序号（0..15）
        public int GolfColorIndex;

        public override void Initialize()
        {
            GolfColorIndex = 0;
        }

        // 供武器调用：取当前索引并自增（循环）
        public int NextIndex()
        {
            int current = GolfColorIndex;
            GolfColorIndex = (GolfColorIndex + 1) % 16; // 4242..4255 共 16 个
            return current;
        }
    }
}
