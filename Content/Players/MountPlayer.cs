using Terraria;
using Terraria.ModLoader;
using WuDao.Content.Mounts;

namespace WuDao.Content.Players
{
    public class MountPlayer : ModPlayer
    {
        public override void PostUpdateEquips()
        {
            if (!Player.mount.Active)
                return;

            int solarMount = ModContent.MountType<SolarShip>();
            int moonMount = ModContent.MountType<LunarShip>();

            // 太阳船：仅白天骑乘时 +10% 伤害减免、近战/远程/投掷伤害
            if (Player.mount.Type == solarMount && Main.dayTime)
            {
                Player.endurance += 0.10f; // 10% DR（伤害减免）

                Player.GetDamage(DamageClass.Melee) += 0.10f;
                Player.GetDamage(DamageClass.Ranged) += 0.10f;

                // 投掷伤害（tML 多数版本有 Throwing 兼容类）
                Player.GetDamage(DamageClass.Throwing) += 0.10f;

                // 如果你的 tML 没有 Throwing（编译报错），用下面替代：改成 Generic 或者把投掷删掉
                // Player.GetDamage(Terraria.ModLoader.DamageClass.Generic) += 0.10f;
            }

            // 月亮船：仅夜晚骑乘时 +20% 魔法、召唤
            if (Player.mount.Type == moonMount && !Main.dayTime)
            {
                Player.GetDamage(DamageClass.Magic) += 0.20f;
                Player.GetDamage(DamageClass.Summon) += 0.20f;
            }
        }
    }
}