using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Buffs
{
    public class ZombieMinionBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.Zombie;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Zombie Minion");
            // Description.SetDefault("A zombie fights for you");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ZombieMinion>()] > 0)
                player.buffTime[buffIndex] = 18000;
            else
                player.DelBuff(buffIndex);
        }
    }
}