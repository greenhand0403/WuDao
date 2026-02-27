using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Mounts
{
    public class LunarShip : ModMount
    {
        private const float VerticalAccel = 0.40f; // 上/下加速度
        private const float VerticalMaxSpeed = 6.00f; // 竖直最大速度
        private const float HoverBrakeFactor = 0.60f; // 悬停刹车
        public override void SetStaticDefaults()
        {
            // 克隆原版海盗船数据
            Mount.MountData mountData = MountData;

            mountData.spawnDust = 228;
            mountData.buff = ModContent.BuffType<LunarShipBuff>();
            mountData.heightBoost = 24;
            mountData.flightTimeMax = int.MaxValue;

            mountData.fallDamage = 0f;
            mountData.usesHover = true;
            mountData.runSpeed = 3f;
            mountData.dashSpeed = 6f;
            mountData.acceleration = 0.12f;
            mountData.jumpHeight = 3;
            mountData.jumpSpeed = 1f;
            mountData.swimSpeed = mountData.runSpeed;

            mountData.totalFrames = 10;
            int[] array = new int[mountData.totalFrames];
            for (int num16 = 0; num16 < array.Length; num16++)
            {
                array[num16] = 9;
            }
            mountData.playerYOffsets = array;
            mountData.xOffset = 0;
            mountData.bodyFrame = 3;
            mountData.yOffset = 8;
            mountData.playerHeadOffset = 16;

            mountData.runningFrameCount = 10;
            mountData.runningFrameDelay = 8;
            mountData.runningFrameStart = 0;
            mountData.idleFrameCount = 10;
            mountData.idleFrameDelay = 8;
            mountData.idleFrameStart = 0;
            mountData.swimFrameCount = 10;
            mountData.swimFrameDelay = 8;
            mountData.swimFrameStart = 0;
            mountData.flyingFrameCount = 10;
            mountData.flyingFrameDelay = 8;
            mountData.flyingFrameStart = 0;
            mountData.inAirFrameCount = 10;
            mountData.inAirFrameDelay = 8;
            mountData.inAirFrameStart = 0;
            mountData.dashingFrameCount = 10;
            mountData.dashingFrameDelay = 8;
            mountData.dashingFrameStart = 0;
            mountData.standingFrameCount = 10;
            mountData.standingFrameDelay = 8;
            mountData.standingFrameStart = 0;

            if (Main.netMode != NetmodeID.Server)
            {
                mountData.backTexture = Asset<Texture2D>.Empty;
                mountData.backTextureExtra = Asset<Texture2D>.Empty;
                // 用自身的贴图
                // mountData.frontTexture = Mod.Assets.Request<Texture2D>($"Content/Mounts/LunarShip_Front", AssetRequestMode.AsyncLoad);
                mountData.frontTextureExtra = Asset<Texture2D>.Empty;
                mountData.textureWidth = mountData.frontTexture.Width();
                mountData.textureHeight = mountData.frontTexture.Height();
            }

            mountData.lightColor = Vector3.One;
            mountData.emitsLight = true;
        }
        public override void SetMount(Player player, ref bool skipDust)
        {
            player.velocity = Vector2.Zero;
            player.dash = 0;
            player.dashType = 0;
            player.dashDelay = 0;
            player.dashTime = 0;
        }
        public override void UpdateEffects(Player player)
        {
            player.gravity = 0f;
            bool ascend = player.controlUp || player.controlJump; // 上键或空格都向上
            bool descend = player.controlDown;
            if (ascend && !descend)
            {
                player.velocity.Y = MathHelper.Clamp(player.velocity.Y - VerticalAccel, -VerticalMaxSpeed, VerticalMaxSpeed);
            }
            else if (descend && !ascend)
            {
                player.velocity.Y = MathHelper.Clamp(player.velocity.Y + VerticalAccel, -VerticalMaxSpeed, VerticalMaxSpeed);
            }
            else
            {
                // 没按上下时，缓慢刹车到悬停
                player.velocity.Y *= HoverBrakeFactor;
                if (Math.Abs(player.velocity.Y) < 0.05f)
                    player.velocity.Y = 0f;
            }
        }
    }
}