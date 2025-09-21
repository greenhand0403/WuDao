using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Players;

namespace WuDao.Content.Buffs
{
    class GelFlaskBuff : ModBuff
    {
        public override string Texture => $"Terraria/Images/Buff_{BuffID.WeaponImbueIchor}";
        public override void SetStaticDefaults()
        {
            BuffID.Sets.IsAFlaskBuff[Type] = true;
            Main.meleeBuff[Type] = true;
            Main.persistentBuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<WeaponEnchantmentPlayer>().GelFlaskImbue = true;
            player.MeleeEnchantActive = true; // MeleeEnchantActive indicates to other mods that a weapon imbue is active.
        }
    }
}
