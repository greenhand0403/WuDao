using Terraria;
using Terraria.ModLoader;
using WuDao.Content.Items.Accessories;

namespace WuDao.Content.Players
{
    public class BoomerangAccessoryPlayer : ModPlayer
    {
        public bool Yanfan;   // 燕返：回程伤害翻倍
        public bool Guixin;   // 归心似箭：回程速度=外放2倍
        public bool Yuesheng; // 跃升：穿墙+无限穿透

        public override void ResetEffects()
        {
            Yanfan = false;
            Guixin = false;
            Yuesheng = false;
        }
    }
}
