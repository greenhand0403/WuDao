using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Global.Systems
{
    // 模仿者 特殊魔法武器的辅助类 解锁定义：指定NPC -> 解锁的友方射弹 -> 所需击杀数
    public struct UnlockDef
    {
        public int NpcType;
        public int ProjectileType;
        public int Required;
        public string DisplayName; // 用于提示

        public UnlockDef(int npcType, int projType, int required, string display)
        {
            NpcType = npcType;
            ProjectileType = projType;
            Required = required;
            DisplayName = display;
        }
    }

    public class MimickerSystem : ModSystem
    {
        // 你可以根据需要增删，尽量选择“发射弹幕的远程/施法型敌怪”
        public static readonly UnlockDef[] UnlockTable = new UnlockDef[]
        {
            // 例：击败 Ichor Sticker(灵液黏黏怪) 解锁 黄金雨 友方弹
            new UnlockDef(NPCID.IchorSticker, ProjectileID.GoldenShowerFriendly, 1, "黄金雨"),
            // 腐化者（喷吐咒火弹） -> 诅咒焰
            new UnlockDef(NPCID.Corruptor, ProjectileID.CursedFlameFriendly, 1, "诅咒焰"),
            // 地牢死灵施法者 -> 暗影光束
            new UnlockDef(NPCID.Necromancer, ProjectileID.ShadowBeamFriendly, 1, "暗影光束"),
            // 地牢幻魂 -> 幻魂弹（幽灵怨魂）
            new UnlockDef(NPCID.DungeonSpirit, ProjectileID.SpectreWrath, 1, "幽魂之怒"),
            // 地下寒霜法师 -> 水晶风暴
            new UnlockDef(NPCID.IceElemental, ProjectileID.CrystalStorm, 1, "水晶风暴"),
        };

        public static readonly int[] BasePool = new int[]
        {
            ProjectileID.AmethystBolt,
            ProjectileID.TopazBolt,
            ProjectileID.SapphireBolt,
            ProjectileID.EmeraldBolt,
            ProjectileID.RubyBolt,
            ProjectileID.DiamondBolt,
        };
    }
}