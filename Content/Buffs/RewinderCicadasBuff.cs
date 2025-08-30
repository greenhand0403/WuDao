using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Buffs
{
    public class RewinderCicadasBuff : ModBuff
    {
        public override string Texture => $"WuDao/Content/Items/Accessories/RewinderCicadas";
        public override void SetStaticDefaults()
        {
            DisplayName.Format("Temporal Exhaustion");
            Description.Format("you can't use it in cool down state");
            Main.debuff[Type] = true;        // 作为负面状态显示
            Main.buffNoSave[Type] = false;   // 存档（随存档记忆剩余时间）
            Main.buffNoTimeDisplay[Type] = false; // 显示剩余时间
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 只是显示用，不做额外逻辑
        }
    }
}
