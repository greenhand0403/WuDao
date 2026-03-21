using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace WuDao.Content.Global.Projectiles
{
    public class MimickerGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool fromMimicker;

        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            bitWriter.WriteBit(fromMimicker);
        }

        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            fromMimicker = bitReader.ReadBit();
        }
    }
}