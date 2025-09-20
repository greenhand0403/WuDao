using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using WuDao.Content.Projectiles;

namespace WuDao.Content.Buffs
{
    // 1) Buff：显示为虚饰宠物，维持时间并保证召唤
    public class KyubeyPetBuff : ModBuff
    {
        public override string Texture => "WuDao/Content/Items/Pets/KyubeySoulStone";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; // 不显示计时
            Main.vanityPet[Type] = true;         // 虚饰/宠物 Buff
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            // 如果需要则生成宠物，并将 buff 时间维持在 2 tick（持续刷新）
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused,
                ModContent.ProjectileType<KyubeyPetProjectile>());
        }
    }
}