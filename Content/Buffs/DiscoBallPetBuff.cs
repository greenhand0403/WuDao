using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using WuDao.Content.Global;
using WuDao.Content.Items.Accessories;
using WuDao.Content.Projectiles;

namespace WuDao.Content.Buffs
{
    // 2) BUFF：标记为 LightPet，维持弹幕存在
    public class DiscoBallPetBuff : ModBuff
    {
        // public override string Texture => $"WuDao/Content/Buffs/DiscoBallPetBuff";
        public override string Texture => $"Terraria/Images/Item_{ItemID.DiscoBall}";
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