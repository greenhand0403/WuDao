using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items.Accessories;

namespace WuDao.Content.Enemy
{
    public class FesterMotherDecaySlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4; // 4帧循环
        }

        public override void SetDefaults()
        {
            NPC.width = 174;
            NPC.height = 145;
            NPC.scale = 0.5f;

            NPC.damage = 180;
            NPC.defense = 26;
            NPC.lifeMax = 600;

            NPC.knockBackResist = 0.2f;
            NPC.value = 10f;

            NPC.aiStyle = NPCAIStyleID.Slime;              // 最简单史莱姆AI
            AIType = NPCID.BlueSlime;     // 继承蓝史莱姆行为（跳一跳接近玩家）
            AnimationType = -1;           // 我们自己写4帧循环

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }
        public override void AI()
        {
            NPC.TargetClosest(faceTarget: true);
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
                return;

            if (NPC.localAI[0] > 0f)
                NPC.localAI[0]--;

            float dist = Vector2.Distance(NPC.Center, target.Center);

            if (dist < 600f && NPC.localAI[0] <= 0f)
            {
                FireVileSpitRing_AsNPC(target);
                NPC.localAI[0] = 120f; // 2秒一次，你可以调
            }
        }
        private void FireVileSpitRing_AsNPC(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // 发射点（可按你贴图微调）
            Vector2 shootFrom = NPC.Top + new Vector2(0f, -6f);

            // 看不见玩家则不发射（你要的）
            bool canSee = Collision.CanHit(
                new Vector2(NPC.position.X, NPC.position.Y - 20f),
                NPC.width,
                NPC.height + 20,
                target.position,
                target.width,
                target.height
            );
            if (!canSee)
                return;

            // 4~6道
            int n = Main.rand.Next(4, 7);
            float step = MathHelper.TwoPi / n;

            // 计算玩家方向角（确保其中1道必定指向玩家）
            Vector2 toPlayer = target.Center - shootFrom;
            if (toPlayer.LengthSquared() < 0.001f)
                toPlayer = Vector2.UnitX;

            float aim = toPlayer.ToRotation();

            // 随机选择“哪一道”对准玩家，但整体仍然圆周等分
            int k0 = Main.rand.Next(n);
            float start = aim - k0 * step;

            // 速度（可以随机扰动一点）
            float baseSpeed = Main.rand.NextFloat(7.5f, 9.5f);

            for (int k = 0; k < n; k++)
            {
                float ang = start + k * step;

                float speed = baseSpeed * Main.rand.NextFloat(0.92f, 1.08f);
                Vector2 vel = Vector2.UnitX.RotatedBy(ang) * speed;

                // 生成“魔唾液NPC”
                int idx = NPC.NewNPC(
                    NPC.GetSource_FromAI(),
                    (int)shootFrom.X,
                    (int)shootFrom.Y,
                    NPCID.VileSpit
                );

                if (idx >= 0 && idx < Main.maxNPCs)
                {
                    NPC spit = Main.npc[idx];
                    // 关键：给它速度
                    spit.velocity = vel;
                    // 可选：让它仇恨你的目标（不是必须，但更稳定）
                    spit.target = NPC.target;
                    // 同步
                    spit.netUpdate = true;
                    spit.timeLeft = 270;                // 避免飞太远不消失
                    spit.netUpdate = true;
                }
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Poisoned, 180);
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
        public override void OnKill()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int babyType = ModContent.NPCType<DecaySlime>();

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
            if (spawnInfo.Player.ZoneCorrupt && spawnInfo.Player.ZoneUnderworldHeight && !Main.hardMode)
            {
                return 0.25f;
            }
            return 0f;
        }
        // 10%掉落腐化立方
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CorruptPowerCube>(), 50));
        }
    }
}