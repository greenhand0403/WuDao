using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items.Accessories;

namespace WuDao.Content.Enemy
{
    public class SpikedMotherBloodySlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6; // 6帧动画
        }

        public override void SetDefaults()
        {
            NPC.width = 174;
            NPC.height = 145;
            NPC.scale = 0.5f;

            NPC.damage = 180;
            NPC.defense = 26;
            NPC.lifeMax = 600;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.value = 60f;
            NPC.knockBackResist = 0.2f;

            NPC.aiStyle = NPCAIStyleID.Slime; // 史莱姆AI
            AIType = NPCID.BlueSlime; // 直接继承蓝史莱姆逻辑
            AnimationType = -1; // 我们自己控制动画

            NPC.noGravity = false;
            NPC.noTileCollide = false;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.velocity.Y == 0)
            {
                // 在地面，播放待机两帧
                NPC.frameCounter++;
                if (NPC.frameCounter >= 20)
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0;
                }

                if (NPC.frame.Y >= frameHeight * 2)
                    NPC.frame.Y = 0;
            }
            else
            {
                // 空中阶段
                if (NPC.velocity.Y < 0)
                {
                    NPC.frame.Y = frameHeight * 2; // 起跳
                }
                else if (NPC.velocity.Y < 6f)
                {
                    NPC.frame.Y = frameHeight * 3; // 空中
                }
                else if (NPC.velocity.Y >= 6f)
                {
                    NPC.frame.Y = frameHeight * 4; // 下落
                }
            }
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.TargetClosest();
                target = Main.player[NPC.target];
                // 目标不存在时，快速消失
                NPC.timeLeft = 60; // 很快消失
            }

            float dx = target.Center.X - NPC.Center.X;

            // 在一定距离内，强制方向朝向玩家
            if (Math.Abs(dx) < 700f)
            {
                NPC.direction = dx > 0 ? 1 : -1;
                NPC.spriteDirection = NPC.direction;
            }

            // 可选：像原版一样判断是否能直视玩家（更像尖刺史莱姆）
            bool canHit = Collision.CanHit(
                new Vector2(NPC.position.X, NPC.position.Y - 20f),
                NPC.width,
                NPC.height + 20,
                target.position,
                target.width,
                target.height
            );

            float distance = Vector2.Distance(NPC.Center, target.Center);
            // 仅在地面上才发射弹 && NPC.velocity.Y == 0f
            if (distance < 480f && canHit)
            {
                NPC.localAI[0]++;

                if (NPC.localAI[0] >= 90)
                {
                    NPC.localAI[0] = 0;
                    FireBloodVolley(target);
                }
            }
            else
            {
                NPC.localAI[0] = 0;
            }
        }
        // 触发时调用，比如在距离内且冷却到点时
        private void FireBloodVolley(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // 发射点：NPC正上方
            Vector2 shootFrom = NPC.Top;

            int projType = ProjectileID.BloodShot;
            int damage = 18;
            float knockBack = 1f;

            // 中间那枚：固定速度
            float baseSpeed = 6f + Main.rand.NextFloat(1f, 2f);

            // 计算“指向玩家”的基础速度向量
            Vector2 baseVel = target.Center - shootFrom;
            if (baseVel.LengthSquared() < 0.001f)
                baseVel = Vector2.UnitX;
            baseVel.Normalize();
            baseVel *= baseSpeed;

            // 随机扰动弧度 m（你可以调范围，建议 10°~20°）
            float m = Main.rand.NextFloat(MathHelper.ToRadians(10f), MathHelper.ToRadians(20f));

            // 依次发射：0, +m, +2m, -m, -2m
            float[] rots = { 0f, +m, -m, +2f * m, -2f * m };

            for (int i = 0; i < rots.Length; i++)
            {
                if (i > 2 && Main.rand.NextBool(2))
                    continue;
                // 可选：给“克隆弹”一点速度随机扰动（中间那枚保持固定速度）
                float speedMul = (i == 0) ? 1f : Main.rand.NextFloat(0.92f, 1.08f);

                Vector2 vel = baseVel.RotatedBy(rots[i]) * speedMul;

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    shootFrom,
                    vel,
                    projType,
                    damage,
                    knockBack,
                    Main.myPlayer
                );
            }
        }
        public override void OnKill()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int babyType = ModContent.NPCType<BloodySlime>();

            // 让4个宝宝在尸体附近“十字分布”，避免重叠
            Vector2[] offsets =
            {
                new Vector2(-22f, 0f),
                new Vector2( 22f, 0f),
                new Vector2( 0f, -18f),
                new Vector2( 0f, 18f),
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2 spawnPos = NPC.Center + offsets[i];

                int idx = NPC.NewNPC(
                    NPC.GetSource_Death(),
                    (int)spawnPos.X,
                    (int)spawnPos.Y,
                    babyType
                );

                if (idx >= 0 && idx < Main.maxNPCs)
                {
                    NPC baby = Main.npc[idx];

                    // 给一点初速度，把他们“弹开”，更不容易重叠
                    baby.velocity = offsets[i].SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(2.2f, 3.4f);
                    baby.velocity.Y -= Main.rand.NextFloat(1.0f, 2.0f);

                    baby.netUpdate = true;
                }
            }
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneCrimson && spawnInfo.Player.ZoneUnderworldHeight && Main.hardMode)
            {
                return 0.25f;
            }
            return 0f;
        }
        // 10%掉落猩红立方
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CrimPowerCube>(), 50));
        }
    }
}