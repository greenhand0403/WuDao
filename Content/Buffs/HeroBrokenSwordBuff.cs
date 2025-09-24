using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    public class HeroBrokenSwordBuff : ModBuff
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.BrokenHeroSword}";
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;       // 不存档
            Main.buffNoTimeDisplay[Type] = true;// 不显示计时
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 有小兵则刷新时间；没有则移除（与示例一致）
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Summon.HeroBrokenSwordMinion>()] > 0)
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
