// WuDao/Config/WishingWellConfig.cs
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace WuDao.Content.Config
{
    // 许愿井系统
    public class WishingWellConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Range(10, 90)]
        [Increment(10)]
        public int DoubleRewardPercent = 50;

        public override void OnChanged()
        {
            if (DoubleRewardPercent < 10) DoubleRewardPercent = 10;
            if (DoubleRewardPercent > 90) DoubleRewardPercent = 90;
        }

        public float Chance => DoubleRewardPercent / 100f;
    }
}
