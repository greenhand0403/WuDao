using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using System.IO;

namespace WuDao.Content.Global.Projectiles
{
    // 近视眼镜：用于给所有射弹记录“发射源NPC”的全局组件，后续逻辑是被标记的敌怪造成伤害降低
    public class NearsightedGlobalProjectile : GlobalProjectile
    {
        public int SourceNPC = -1;   // 记录发射者NPC的 whoAmI

        public override bool InstancePerEntity => true;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            SourceNPC = -1;

            if (source is EntitySource_Parent esp && esp.Entity is NPC npc)
            {
                if (npc.active)
                    SourceNPC = npc.whoAmI;
            }
        }
        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(SourceNPC);
        }

        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            SourceNPC = binaryReader.ReadInt32();
        }
    }
}