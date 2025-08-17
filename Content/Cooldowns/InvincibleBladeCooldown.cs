using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Cooldowns
{
    public class InvincibleBladeCooldown : ModPlayer
    {
        public int Cooldown; // “无敌”自定义冷却计时（不吃攻速）
        public override void PostUpdate()
        {
            if (Cooldown > 0) Cooldown--;
        }
    }
}