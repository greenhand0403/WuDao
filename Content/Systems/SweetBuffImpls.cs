
using Terraria;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Systems
{
    /// <summary>
    /// 这三个“甜味”增益直接在 Update 中赋予简易数值，避免额外耦合。
    /// </summary>
    public class SweetRegenUpdate : ModSystem
    {
        public override void PostUpdatePlayers()
        {
            foreach (var p in Main.player)
            {
                if (p != null && p.active)
                {
                    if (p.HasBuff<SweetRegen>())
                    {
                        if (p.lifeRegen < 0) p.lifeRegen = 0;
                        p.lifeRegen += 4; // 约等于每秒 +2 HP
                    }
                    if (p.HasBuff<SweetAgile>())
                    {
                        p.moveSpeed *= 1.15f;
                    }
                    if (p.HasBuff<SweetLucky>())
                    {
                        p.GetCritChance(Terraria.ModLoader.DamageClass.Generic) += 5f; // +5% 暴击
                    }
                }
            }
        }
    }
}
