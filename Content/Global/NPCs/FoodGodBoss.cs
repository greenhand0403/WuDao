using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Global.NPCs
{
    public class FoodGodBoss : ModNPC
    {
        enum Phase { Dash, Cook, Shoot }
        Phase state;
        int timer;
        public override string Texture => $"Terraria/Images/NPC_{NPCID.BrainofCthulhu}";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 90; NPC.height = 120;
            NPC.lifeMax = 4500;               // 接近脑/吞噬者时代
            NPC.damage = 30; NPC.defense = 8;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true; NPC.noTileCollide = true;
            NPC.boss = true; NPC.aiStyle = -1;
            NPC.npcSlots = 10f;
            Music = MusicID.Boss2;
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead) { NPC.TargetClosest(); target = Main.player[NPC.target]; }
            if (!target.active || target.dead) { NPC.velocity.Y -= 0.5f; return; }

            timer++;

            switch (state)
            {
                case Phase.Dash:
                    {
                        // 模仿“冲刺切菜”：对准玩家 3 段加速
                        Vector2 to = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 12f;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, to, 0.08f);
                        if (timer % 40 == 0)
                            ShootFoodArc(target); // 间或抛洒食材
                        if (timer > 180) { timer = 0; state = Phase.Cook; }
                    }
                    break;

                case Phase.Cook:
                    {
                        // 模仿“做菜动作”：原地小幅移动 + 大量食材弹
                        NPC.velocity = new Vector2((float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4) * 2f, (float)System.Math.Cos(Main.GlobalTimeWrappedHourly * 3) * 1f);
                        if (timer % 12 == 0) ShootFoodCone(target, 10);
                        if (timer > 180) { timer = 0; state = Phase.Shoot; }
                    }
                    break;

                case Phase.Shoot:
                    {
                        // 模仿“圣诞老人机枪”：向玩家扫射快落体（有红/绿混合）
                        if (timer % 6 == 0)
                        {
                            bool red = Main.rand.NextBool(2);
                            NewFoodBullet(NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(7f, 10f), red);
                        }
                        if (timer > 180) { timer = 0; state = Phase.Dash; }
                    }
                    break;
            }
        }

        void ShootFoodArc(Player target)
        {
            int n = 6;
            for (int i = 0; i < n; i++)
            {
                float ang = MathHelper.ToRadians(-40 + 80f * i / (n - 1));
                var v = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).RotatedBy(ang) * 9f;
                NewFoodBullet(NPC.Center, v, red: i % 2 == 0);
            }
        }
        void ShootFoodCone(Player target, int n)
        {
            float spread = MathHelper.ToRadians(30);
            for (int i = 0; i < n; i++)
            {
                float lerp = (i / (float)(n - 1) - 0.5f);
                var v = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX).RotatedBy(lerp * spread) * Main.rand.NextFloat(6f, 9f);
                NewFoodBullet(NPC.Center, v, red: Main.rand.NextBool(3)); // 少量红，多数绿
            }
        }

        void NewFoodBullet(Vector2 pos, Vector2 vel, bool red)
        {
            Projectile.NewProjectile(null, pos, vel,
                ModContent.ProjectileType<FoodRainProjectile>(),
                red ? 20 : 0, 0f, Main.myPlayer,
                red ? 1f : 0f, ItemID.Sashimi);
        }

        public override void OnKill()
        {
            // 掉落略：可加食材、食谱等
        }
    }
}