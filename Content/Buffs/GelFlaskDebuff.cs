using Terraria.ModLoader;
using Terraria.ID;

namespace WuDao.Content.Buffs
{
    // TODO: 更换贴图 凝胶减益debuff
    class GelFlaskDebuff : ModBuff
    {
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Slow}";
    }
}
