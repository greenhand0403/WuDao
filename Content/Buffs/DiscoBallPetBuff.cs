using Terraria.ModLoader;
using Terraria;
using WuDao.Content.Projectiles;

namespace WuDao.Content.Buffs
{
    // 迪斯科灯球宠物BUFF：标记为 LightPet，维持弹幕存在
    public class DiscoBallPetBuff : ModBuff
    {
        public override string Texture => $"WuDao/Content/Items/Pets/DiscoBallRemote";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.lightPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ModContent.ProjectileType<DiscoBallPetProj>());
        }
    }
}