using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;

namespace WuDao.Content.Buffs
{
    public class SheRaTransformBuff : ModBuff
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Melee/SheRaSword";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            var modPlayer = player.GetModPlayer<SheRaSwordPlayer>();

            // 如果变身已经结束，移除 Buff
            if (!modPlayer.IsTransformed)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}