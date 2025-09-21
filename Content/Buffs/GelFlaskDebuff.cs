using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Players;
using WuDao.Content.Global.NPCs;

namespace WuDao.Content.Buffs
{
    class GelFlaskDebuff : ModBuff
    {
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Slow}";
    }
}
