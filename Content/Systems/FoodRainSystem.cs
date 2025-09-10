using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Global.NPCs;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Systems
{
    public class FoodRainSystem : ModSystem
    {
        // —— 事件状态 ——
        public static bool Active;
        public static int TimeLeft;          // 帧
        public static int PostWinDelay;      // 成功后 10 秒倒计时
        public static int Owner;             // 发起者（掉落/生成时归属）
        const int DURATION = 60 * 180;       // 3 分钟

        public override void OnWorldLoad()
        {
            Active = false; TimeLeft = 0; PostWinDelay = 0; Owner = -1;
        }

        public static void TryTrigger(Player p)
        {
            if (Active) return;
            // 海边 & 10% 几率
            if (p.ZoneBeach && Main.rand.NextFloat() < 0.10f)
            {
                Active = true; TimeLeft = DURATION; PostWinDelay = 0; Owner = p.whoAmI;
                if (Main.netMode != NetmodeID.Server)
                    Main.NewText("食物海涌来啦！躲开红光，吃下绿光！", 255, 180, 80);
            }
        }

        public override void PostUpdateEverything()
        {
            if (!Active) return;

            var p = Main.player[Owner];
            // 玩家死亡提前结束
            if (!p.active || p.dead)
            {
                Active = false; TimeLeft = 0; PostWinDelay = 0; return;
            }

            // 持续刷“食物子弹雨”
            if (TimeLeft > 0)
            {
                TimeLeft--;
                if (Main.GameUpdateCount % 6 == 0) // 频率
                    SpawnOneWave(p);

                if (TimeLeft == 0)
                {
                    // 胜利，准备 10 秒后召唤 Boss
                    PostWinDelay = 60 * 10;
                    if (Main.netMode != NetmodeID.Server)
                        Main.NewText("你撑过了食物海！食神即将到来……", 255, 220, 120);
                }
            }
            else if (PostWinDelay > 0)
            {
                if (--PostWinDelay == 0)
                {
                    // 召唤 Boss（在玩家附近）
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        var pos = p.Center + new Vector2(0, -600);
                        NPC.NewNPC(null, (int)pos.X, (int)pos.Y, ModContent.NPCType<FoodGodBoss>());
                    }
                    Active = false;
                }
            }
        }

        private static readonly int[] PoolFoodItemIDs = new int[] {
        ItemID.CookedShrimp, ItemID.CookedFish, ItemID.PumpkinPie, ItemID.Burger,
        ItemID.Sashimi, ItemID.FruitJuice, ItemID.Escargot,
        ItemID.GoldenDelight
    };

        private static void SpawnOneWave(Player p)
        {
            // 在玩家上方随机 X 生成若干“红/绿”两圈光的食物弹
            int count = Main.rand.Next(3, 6);
            for (int i = 0; i < count; i++)
            {
                int foodItemId = PoolFoodItemIDs[Main.rand.Next(PoolFoodItemIDs.Length)];
                bool harmful = Main.rand.NextBool(); // 红：伤害；绿：治疗
                var spawn = p.Center + new Vector2(Main.rand.NextFloat(-800f, 800f), -600f);
                var vel = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(6f, 9f));

                int proj = Projectile.NewProjectile(
                    spawnSource: p.GetSource_Misc("FoodRain"),
                    position: spawn,
                    velocity: vel,
                    Type: ModContent.ProjectileType<FoodRainProjectile>(),
                    Damage: harmful ? 25 : 0,
                    KnockBack: 0f,
                    Owner: Owner,
                    ai0: harmful ? 1f : 0f,     // ai0=1 红，=0 绿
                    ai1: foodItemId             // 用 ai1 存展示用的物品 ID
                );
            }
        }
    }
}