using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics; // ★ 关键：DynamicSpriteFont
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Global.NPCs;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Systems
{
    public class FoodRainSystem : ModSystem
    {
        public static bool Active;
        public static int TimeLeft;
        public static int PostWinDelay;
        public static int Owner;
        const int DURATION = 60 * 60; // 1 分钟

        public override void OnWorldLoad()
        {
            Active = false; TimeLeft = 0; PostWinDelay = 0; Owner = -1;
        }

        public static void TryTrigger(Player p)
        {
            if (Active) return;
            if (p.ZoneBeach && Main.rand.NextFloat() < 0.90f)
            {
                Active = true;
                TimeLeft = DURATION;
                PostWinDelay = 0;
                Owner = p.whoAmI;
                if (Main.netMode != NetmodeID.Server)
                    Main.NewText("食物从天而降！躲开红光，吃下绿光！", 255, 180, 80);
            }
        }

        public override void PostUpdateEverything()
        {
            if (!Active) goto AfterActive;

            var p = Main.player[Owner];
            if (!p.active || p.dead)
            {
                Active = false; TimeLeft = 0; PostWinDelay = 0; return;
            }

            if (TimeLeft > 0)
            {
                TimeLeft--;
                // 生成射弹的帧间隔
                if (Main.GameUpdateCount % 12 == 0)
                    SpawnOneWave(p);

                if (TimeLeft == 0)
                {
                    // ★ 关键修复：事件结束，关闭 Active，进入胜利倒计时分支
                    Active = false;
                    PostWinDelay = 60 * 10;
                    if (Main.netMode != NetmodeID.Server)
                        Main.NewText("你撑过了食物海！食神即将到来……", 255, 220, 120);
                }
            }

        AfterActive:
            if (!Active && PostWinDelay > 0)
            {
                var p2 = Main.player[Owner];
                if (!p2.active || p2.dead) { PostWinDelay = 0; return; }

                if (--PostWinDelay == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        var pos = p2.Center + new Vector2(0, -600);
                        NPC.NewNPC(null, (int)pos.X, (int)pos.Y, ModContent.NPCType<FoodGodBoss>());
                    }
                }
            }
        }

        private static void SpawnOneWave(Player p)
        {
            // 一波射弹的密度
            int count = Main.rand.Next(2, 4);
            for (int i = 0; i < count; i++)
            {
                int foodItemId = Helpers.GetRandomFromSet(ItemID.Sets.IsFood);
                bool harmful = Main.rand.NextBool();
                var spawn = p.Center + new Vector2(Main.rand.NextFloat(-800f, 800f), -600f);
                var vel = new Vector2(Main.rand.NextFloat(-2.1f, 2.1f), Main.rand.NextFloat(3.4f, 6.8f));

                Projectile.NewProjectile(
                    p.GetSource_Misc("FoodRain"), spawn, vel,
                    ModContent.ProjectileType<FoodRainProjectile>(),
                    harmful ? 5 : 0, 0f, Owner,
                    harmful ? 1f : 0f, foodItemId
                );
            }
        }

        // ===== 事件进度条 UI =====
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (Main.netMode == NetmodeID.Server) return;

            bool showDoing = Active && TimeLeft > 0;
            bool showWinCountdown = !Active && PostWinDelay > 0;
            if (!showDoing && !showWinCountdown) return;

            Vector2 pos = new Vector2(Main.screenWidth / 2f, 84f);
            const int width = 360, height = 16;

            float percent;
            string title, sub;

            if (showDoing)
            {
                percent = MathHelper.Clamp(1f - TimeLeft / (float)DURATION, 0f, 1f);
                title = "食物海事件";
                sub = $"抵挡来袭：{(int)(percent * 100)}%   剩余 {FormatTime(TimeLeft)}";
            }
            else
            {
                percent = MathHelper.Clamp(1f - PostWinDelay / 600f, 0f, 1f);
                title = "胜利！食神即将到来";
                sub = $"召唤倒计时：{(PostWinDelay / 60f):0.0}s";
            }

            DrawBar(spriteBatch, pos, width, height, percent, title, sub);
        }

        private static void DrawBar(SpriteBatch sb, Vector2 pos, int w, int h, float p, string title, string sub)
        {
            Texture2D pix = TextureAssets.MagicPixel.Value;

            Rectangle back = new Rectangle((int)(pos.X - w / 2), (int)(pos.Y - h / 2), w, h);
            sb.Draw(pix, back, new Color(0, 0, 0, 180));

            sb.Draw(pix, new Rectangle(back.X - 2, back.Y - 2, back.Width + 4, 2), Color.Black);
            sb.Draw(pix, new Rectangle(back.X - 2, back.Bottom, back.Width + 4, 2), Color.Black);
            sb.Draw(pix, new Rectangle(back.X - 2, back.Y, 2, back.Height), Color.Black);
            sb.Draw(pix, new Rectangle(back.Right, back.Y, 2, back.Height), Color.Black);

            int fill = (int)(back.Width * p);
            if (fill > 0)
                sb.Draw(pix, new Rectangle(back.X, back.Y, fill, back.Height), new Color(255, 120, 80, 255));

            Vector2 titleSize = FontAssets.MouseText.Value.MeasureString(title);
            Vector2 subSize = FontAssets.MouseText.Value.MeasureString(sub);
            Vector2 titlePos = new Vector2(pos.X - titleSize.X / 2f, back.Y - titleSize.Y - 4);
            Vector2 subPos = new Vector2(pos.X - subSize.X / 2f, back.Bottom + 4);

            DrawStringOutlined(FontAssets.MouseText.Value, title, titlePos, Color.Gold, Color.Black);
            DrawStringOutlined(FontAssets.MouseText.Value, sub, subPos, Color.White, Color.Black);
        }

        private static void DrawStringOutlined(DynamicSpriteFont font, string text, Vector2 pos, Color color, Color outline)
        {
            Main.spriteBatch.DrawString(font, text, pos + new Vector2(1, 1), outline);
            Main.spriteBatch.DrawString(font, text, pos, color);
        }

        private static string FormatTime(int frames)
        {
            int totalSec = (int)System.Math.Ceiling(frames / 60f);
            return $"{totalSec / 60:00}:{totalSec % 60:00}";
        }
    }
}
