using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Projectiles.Throwing;
using Terraria.Audio;
using ReLogic.Utilities;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using WuDao.Content.Items;

namespace WuDao.Content.Global.NPCs
{
    [AutoloadBossHead]
    public class FoodGodBoss : ModNPC
    {
        enum Phase { Dash, Cook, Shoot }
        Phase state = Phase.Dash;
        int timer;
        int dashesDone;

        // ===== 动画 =====
        int frameTimer = 0, currentFrame = 0, currentLoopStart = 0;
        const int framesPerSegment = 3;  // ★ 修正为3，对应 0~2 / 3~5
        const int frameTick = 6;
        private float spawnAlpha = 1f;
        private bool isDying = false;
        private int fadeTimer = 60; // 1秒淡出
        public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 6;
        public override void SetDefaults()
        {
            NPC.width = 90;
            NPC.height = 120;
            NPC.lifeMax = 1500;
            NPC.damage = 10;
            NPC.defense = 4;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.npcSlots = 10f;
            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Michikusa2");
        }
        public override void AI()
        {
            if (isDying)
            {
                fadeTimer--;
                NPC.alpha = (int)MathHelper.Lerp(0, 255, 1f - fadeTimer / 60f);
                if (fadeTimer <= 0)
                {
                    NPC.life = 0;
                    NPC.checkDead(); // 触发真正死亡
                }
                return;
            }

            Player target = Main.player[NPC.target];
            if (!target.active || target.dead) { NPC.TargetClosest(); target = Main.player[NPC.target]; }
            if (!target.active || target.dead) { NPC.velocity.Y -= 0.5f; return; }

            const int MaxDashes = 2;
            const int DashDuration = 18;
            const int DashPause = 15;
            timer++;

            switch (state)
            {
                case Phase.Dash:
                    {
                        // 始终朝向玩家
                        NPC.spriteDirection = NPC.direction = (target.Center.X >= NPC.Center.X) ? 1 : -1;

                        // ★ Dash 起手（timer==1）若距离过远，先瞬移到玩家附近再冲刺
                        if (timer == 1)
                        {
                            float farDist = 600f;
                            if (Vector2.Distance(NPC.Center, target.Center) > farDist)
                            {
                                for (int tries = 0; tries < 20; tries++)
                                {
                                    // 玩家周围随机圆环点（360~520px）
                                    Vector2 offset = Main.rand.NextVector2Unit() * Main.rand.Next(360, 520);
                                    Vector2 candidate = target.Center + offset;
                                    Rectangle hitbox = new Rectangle((int)candidate.X - NPC.width / 2, (int)candidate.Y - NPC.height / 2, NPC.width, NPC.height);

                                    if (!Collision.SolidCollision(hitbox.TopLeft(), hitbox.Width, hitbox.Height))
                                    {
                                        NPC.Center = candidate;
                                        break;
                                    }
                                }
                            }

                            // 确认朝向并给速度开始第一次冲刺
                            Vector2 dir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                            float speed = 16f + (NPC.life < NPC.lifeMax / 2 ? 4f : 0f);
                            NPC.velocity = dir * speed;
                            dashesDone++;
                        }

                        if (timer <= DashDuration)
                        {
                            if (timer == 6) ShootFoodArc(target, 6, 85f, 7f);
                        }
                        else if (timer <= DashDuration + DashPause)
                        {
                            NPC.velocity *= 0.90f;
                        }
                        else
                        {
                            if (dashesDone >= MaxDashes)
                            {
                                dashesDone = 0;
                                state = Phase.Cook;
                                timer = 0;
                                NPC.velocity = Vector2.Zero;
                            }
                            else
                            {
                                // 下一次冲刺
                                timer = 0;
                            }
                        }
                        break;
                    }

                case Phase.Cook:
                    {
                        // ★ 原地不动但始终面向玩家
                        NPC.spriteDirection = NPC.direction = (target.Center.X >= NPC.Center.X) ? 1 : -1;

                        NPC.velocity = Vector2.Zero;
                        // 建议：频率改为每 18 帧发一次，数量减到 2，但扩大散布到 40°
                        if (timer % 18 == 0)
                            ShootFoodCone(target, 2, 40f, 4f, 10f);
                        if (timer > 120)
                        {
                            timer = 0;
                            state = Phase.Shoot;
                        }
                        break;
                    }

                case Phase.Shoot:
                    {
                        // 缓慢逼近 + 点射，同时也面向玩家（可选）
                        NPC.spriteDirection = NPC.direction = (target.Center.X >= NPC.Center.X) ? 1 : -1;

                        Vector2 to = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 3.0f;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, to, 0.08f);

                        if (timer % 12 == 0)
                        {
                            bool red = Main.rand.NextBool(2);
                            NewFoodBullet(NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4f, 8f), red);
                        }
                        if (timer > 120)
                        {
                            timer = 0;
                            state = Phase.Dash;
                        }
                        break;
                    }
            }
        }

        // ===== 发射工具 =====
        void ShootFoodArc(Player target, int n, float halfAngleDeg, float speed)
        {
            for (int i = 0; i < n; i++)
            {
                float t = n == 1 ? 0f : i / (float)(n - 1);
                float ang = MathHelper.ToRadians(-halfAngleDeg + (halfAngleDeg * 2f) * t);
                var v = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).RotatedBy(ang) * speed;
                NewFoodBullet(NPC.Center, v, red: i % 2 == 0);
            }
        }

        void ShootFoodCone(Player target, int n, float spreadDeg, float minSpd, float maxSpd)
        {
            float spread = MathHelper.ToRadians(spreadDeg);
            for (int i = 0; i < n; i++)
            {
                float lerp = (i / (float)(n - 1) - 0.5f);
                var v = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX).RotatedBy(lerp * spread) * Main.rand.NextFloat(minSpd, maxSpd);
                NewFoodBullet(NPC.Center, v, red: Main.rand.NextBool(3));
            }
        }

        void NewFoodBullet(Vector2 pos, Vector2 vel, bool red)
        {
            Projectile.NewProjectile(
                null, pos, vel,
                ModContent.ProjectileType<FoodRainProjectile>(),
                red ? 20 : 0, 0f, Main.myPlayer,
                red ? 1f : 0f,
                Helpers.GetRandomFromSet(ItemID.Sets.IsFood)
            );
        }

        // ===== 动画（>50% 用0~2；<=50% 用3~5） =====
        public override void FindFrame(int frameHeight)
        {
            int desiredStart = (NPC.life <= NPC.lifeMax / 2) ? 3 : 0;
            if (currentLoopStart != desiredStart)
            {
                currentLoopStart = desiredStart;
                currentFrame = desiredStart;
                frameTimer = 0;
            }
            if (++frameTimer >= frameTick)
            {
                frameTimer = 0;
                currentFrame++;
                if (currentFrame >= currentLoopStart + framesPerSegment)
                    currentFrame = currentLoopStart;
            }

            NPC.frame.Width = TextureAssets.Npc[NPC.type].Value.Width;
            NPC.frame.Height = frameHeight;
            NPC.frame.X = 0;
            NPC.frame.Y = currentFrame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            Rectangle src = NPC.frame;
            Vector2 origin = new Vector2(src.Width / 2f, src.Height / 2f);
            SpriteEffects fx = (NPC.spriteDirection == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            // alpha 从 0 → 1
            float alpha = MathHelper.Clamp(1f - spawnAlpha, 0f, 1f);
            spawnAlpha -= 0.01f; // 逐渐降低
            Main.EntitySpriteDraw(
                tex,
                NPC.Center - Main.screenPosition,
                src,
                drawColor * alpha,
                NPC.rotation,
                origin,
                NPC.scale,
                fx,
                0
            );
            return false;
        }
        public override bool CheckDead()
        {
            if (!isDying)
            {
                isDying = true;
                NPC.dontTakeDamage = true;
                NPC.life = 1;
                NPC.timeLeft = 2;
                return false; // 阻止立即死亡
            }
            return true;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // TODO: 未测试能否正常掉落宝藏袋
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<FoodGodBossBag>()));
        }
    }
}
