using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Players
{
    // 无敌
    public class InvincibleBladePlayer : ModPlayer
    {
        public int ExtraSpawnCD;
        public int Cooldown; // “无敌”自定义冷却计时（不吃攻速）
        public override void PostUpdate()
        {
            if (Cooldown > 0) Cooldown--;
            if (ExtraSpawnCD > 0) ExtraSpawnCD--;
        }
    }
}