using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Buffs
{
    public class MonsterTrainBuff : ModBuff
    {
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Pygmies}";
        public override void SetStaticDefaults()
        {
            // 召唤类 Buff 的基本设置
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<MonsterTrainMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}