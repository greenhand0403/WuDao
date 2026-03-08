using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WuDao.Content.Mounts
{
    public class YuJianMount : ModMount
    {
        public override void SetStaticDefaults()
        {
            MountData.spawnDust = 0;
            MountData.buff = 0; // 不需要 buff
            MountData.heightBoost = 0;

            MountData.fallDamage = 0f;
            MountData.runSpeed = 12f;     // 比天界星盘更快一点（你可再调）
            MountData.dashSpeed = 12f;
            MountData.acceleration = 0.5f;

            MountData.jumpHeight = 0;
            MountData.jumpSpeed = 0f;

            MountData.flightTimeMax = int.MaxValue;
            MountData.fatigueMax = int.MaxValue;
            MountData.constantJump = true;

            MountData.totalFrames = 1;
            MountData.playerYOffsets = new int[] { 0 };
            MountData.xOffset = 0;
            MountData.yOffset = 0;

            MountData.usesHover = true;
            MountData.swimSpeed = 12f;

            MountData.emitsLight = true;
            MountData.lightColor = new Vector3(1f, 1f, 1f);
        }
    }
}