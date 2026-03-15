using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Mounts
{
    public class DuckMount : ModMount
    {
        public override void SetStaticDefaults()
        {
            MountData.spawnDust = DustID.Cloud;

            // 绑定 Buff
            MountData.buff = ModContent.BuffType<Buffs.DuckMountBuff>();

            // -------- 基础参数 --------

            MountData.heightBoost = 10;
            MountData.fallDamage = 0f;
            MountData.runSpeed = 5.5f;
            MountData.dashSpeed = 4f;
            MountData.acceleration = 0.18f;

            // 这里不要禁用跳跃，否则没法主动起飞
            // 改成“小跳一下就能起飞”
            MountData.jumpHeight = 5;
            MountData.jumpSpeed = 4f;

            MountData.blockExtraJumps = true;

            // 比蜜蜂更短
            MountData.flightTimeMax = 70;
            MountData.fatigueMax = 0;

            // 可游泳
            MountData.swimSpeed = 4f;

            MountData.totalFrames = 15;

            // -------- 玩家坐姿偏移 --------

            MountData.playerYOffsets = new int[MountData.totalFrames];
            for (int i = 0; i < MountData.totalFrames; i++)
            {
                MountData.playerYOffsets[i] = 16;
            }

            MountData.xOffset = 3;
            MountData.yOffset = 2;
            MountData.playerHeadOffset = 15;
            MountData.bodyFrame = 3;

            // -------- 帧分配 --------
            // 第1帧      -> standing / idle
            // 第2~3帧    -> 水面游泳
            // 第4~11帧   -> 地面跑步
            // 第12~15帧  -> 飞行 / 空中

            // 站立
            MountData.standingFrameCount = 1;
            MountData.standingFrameDelay = 12;
            MountData.standingFrameStart = 0;
            
            // 静止
            MountData.idleFrameCount = 1;
            MountData.idleFrameDelay = 12;
            MountData.idleFrameStart = 0;

            // 水面游泳与飞行相同
            MountData.swimFrameCount = 4;
            MountData.swimFrameDelay = 9;
            MountData.swimFrameStart = 11;

            // 地面跑步：第4~11帧
            MountData.runningFrameCount = 8;
            MountData.runningFrameDelay = 8;
            MountData.runningFrameStart = 3;

            // 飞行：第12~15帧
            MountData.inAirFrameCount = 4;
            MountData.inAirFrameDelay = 7;
            MountData.inAirFrameStart = 11;

            MountData.flyingFrameCount = 4;
            MountData.flyingFrameDelay = 7;
            MountData.flyingFrameStart = 11;

            // -------- 贴图尺寸 --------
            // 单帧 66x52，竖排15帧
            MountData.textureWidth = 66;
            MountData.textureHeight = 52 * 15;
        }

        public override void UpdateEffects(Player player)
        {
            // 在水里手感轻一点
            if (player.wet)
            {
                if (player.velocity.Y > 2f)
                    player.velocity.Y *= 0.9f;
            }

            // 不要再阻止跳跃上升，否则没法起飞
        }
    }
}