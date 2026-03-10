using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Enemy
{
    public class DoomGnome : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.Gnome];
        }

        public override void SetDefaults()
        {
            NPC.width = 14;
            NPC.height = 30;

            NPC.damage = 15;
            NPC.defense = 2;
            NPC.lifeMax = 100;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.value = 200f;
            NPC.knockBackResist = 0.8f;

            NPC.aiStyle = -1; // 自定义AI，不用原版侏儒AI
            AnimationType = NPCID.Gnome;

            NPC.noGravity = false;
            NPC.noTileCollide = false;
        }

        public override void AI()
        {
            NPC.TargetClosest(faceTarget: true);
            Player player = Main.player[NPC.target];

            if (!player.active || player.dead)
            {
                NPC.velocity.X *= 0.9f;
                if (NPC.timeLeft > 60)
                    NPC.timeLeft = 60;
                return;
            }

            float speed = 2.4f;
            float accel = 0.08f;

            // 朝玩家移动
            if (player.Center.X < NPC.Center.X)
            {
                NPC.direction = -1;
                NPC.spriteDirection = -1;

                if (NPC.velocity.X > -speed)
                    NPC.velocity.X -= accel;
            }
            else
            {
                NPC.direction = 1;
                NPC.spriteDirection = 1;

                if (NPC.velocity.X < speed)
                    NPC.velocity.X += accel;
            }

            // 落地时，遇墙跳
            if (NPC.collideX && NPC.velocity.Y == 0f)
            {
                NPC.velocity.Y = -7f;
            }

            // 简单检测前方坑洞，避免傻站着掉下去
            if (NPC.velocity.Y == 0f)
            {
                int frontX = (int)((NPC.Center.X + 12 * NPC.direction) / 16f);
                int footY = (int)((NPC.Bottom.Y + 8f) / 16f);

                if (WorldGen.InWorld(frontX, footY, 1))
                {
                    Tile tile = Main.tile[frontX, footY];
                    bool hasGround = tile.HasTile && Main.tileSolid[tile.TileType];

                    if (!hasGround)
                    {
                        NPC.velocity.Y = -6.5f;
                    }
                }
            }

            // 限制横向速度
            if (NPC.velocity.X > speed)
                NPC.velocity.X = speed;
            if (NPC.velocity.X < -speed)
                NPC.velocity.X = -speed;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            int[] debuffs = new int[]
            {
                BuffID.Poisoned,
                BuffID.OnFire,
                BuffID.Weak,
                BuffID.Slow,
                BuffID.Bleeding,
                BuffID.Darkness,
                BuffID.Cursed
            };

            int debuff = Main.rand.Next(debuffs);
            target.AddBuff(debuff, 120); // 2秒

            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                int type = target.buffType[i];

                if (type > 0 && Main.debuff[type])
                {
                    target.buffTime[i] += 300; // +5秒
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!Main.dayTime && IsNearLivingTree(spawnInfo.Player))
            {
                return 0.12f;
            }

            return 0f;
        }

        private bool IsNearLivingTree(Player player)
        {
            int tileX = (int)(player.Center.X / 16f);
            int tileY = (int)(player.Center.Y / 16f);

            for (int x = tileX - 30; x < tileX + 30; x++)
            {
                for (int y = tileY - 40; y < tileY + 40; y++)
                {
                    if (!WorldGen.InWorld(x, y))
                        continue;

                    Tile tile = Main.tile[x, y];
                    if (tile.HasTile && tile.TileType == TileID.LivingWood)
                        return true;
                }
            }

            return false;
        }
    }
}