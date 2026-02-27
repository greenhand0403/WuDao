using Terraria;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Buffs
{
    // 怪物火车 Summon Buff
    public class MonsterTrainBuff : ModBuff
    {
        public override string Texture => $"WuDao/Content/Items/Weapons/Summon/MonsterTrainStaff";
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