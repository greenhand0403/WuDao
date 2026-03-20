using Terraria;
using Terraria.ID;
using WuDao.Content.Global;
using Terraria.ModLoader;

namespace WuDao.Content.Items
{
    public class TimeStopItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 34;
            Item.useAnimation = 34;
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item4;
        }

        public override bool CanUseItem(Player player)
        {
            if (TimeStopSystem.IsOnCooldown)
                return false;

            if (TimeStopSystem.IsFrozen)
                return false;

            return true;
        }

        public override bool? UseItem(Player player)
        {
            const int duration = 300;
            const int cooldown = 2400;

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                bool ok = TimeStopSystem.TryStartFreeze(duration, cooldown, FreezeScope.Global, -1);
                return ok;
            }

            if (player.whoAmI == Main.myPlayer)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)MessageType.RequestTimeStop);
                packet.Write((byte)FreezeScope.Global);
                packet.Write(duration);
                packet.Write(cooldown);
                packet.Write((byte)255); // allowedPlayer = -1
                packet.Send();
            }

            return true;
        }
    }
}