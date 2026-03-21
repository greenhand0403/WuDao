using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics; // ★ 关键：DynamicSpriteFont
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using WuDao.Common;
using WuDao.Content.Global.NPCs;
using WuDao.Content.Projectiles.Throwing;

namespace WuDao.Content.Systems
{
    // 食物雨事件触发系统
    public class FoodRainSystem : ModSystem
    {
        public static bool Active;
        public static int TimeLeft;
        public static int PostWinDelay;
        public static int Owner;
        const int DURATION = 60 * 60; // 1 分钟

        public override void OnWorldLoad()
        {
            Active = false;
            TimeLeft = 0;
            PostWinDelay = 0;
            Owner = -1;
        }
        public override void ClearWorld()
        {
            Active = false;
            TimeLeft = 0;
            PostWinDelay = 0;
            Owner = -1;
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["FoodRainActive"] = Active;
            tag["FoodRainTimeLeft"] = TimeLeft;
            tag["FoodRainPostWinDelay"] = PostWinDelay;
            tag["FoodRainOwner"] = Owner;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            Active = tag.GetBool("FoodRainActive");
            TimeLeft = tag.GetInt("FoodRainTimeLeft");
            PostWinDelay = tag.GetInt("FoodRainPostWinDelay");
            Owner = tag.GetInt("FoodRainOwner");

            if (Owner < 0 || Owner >= Main.maxPlayers)
            {
                Active = false;
                TimeLeft = 0;
                PostWinDelay = 0;
                Owner = -1;
            }
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Active);
            writer.Write(TimeLeft);
            writer.Write(PostWinDelay);
            writer.Write(Owner);
        }

        public override void NetReceive(BinaryReader reader)
        {
            bool oldActive = Active;
            int oldPostWinDelay = PostWinDelay;

            Active = reader.ReadBoolean();
            TimeLeft = reader.ReadInt32();
            PostWinDelay = reader.ReadInt32();
            Owner = reader.ReadInt32();

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (!oldActive && Active)
                {
                    Main.NewText(Language.GetTextValue("Mods.WuDao.Events.FoodRain.Start"), 255, 180, 80);
                }
                else if (oldActive && !Active && PostWinDelay > 0 && oldPostWinDelay <= 0)
                {
                    Main.NewText(Language.GetTextValue("Mods.WuDao.Events.FoodRain.Win"), 255, 220, 120);
                }
            }
        }
        public static void TryTrigger(Player p)
        {
            if (Active)
                return;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (!p.active || p.dead)
                return;

            if (!p.ZoneBeach)
                return;

            if (Main.rand.NextFloat() >= 0.33f)
                return;

            Active = true;
            TimeLeft = DURATION;
            PostWinDelay = 0;
            Owner = p.whoAmI;

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(Language.GetTextValue("Mods.WuDao.Events.FoodRain.Start"), 255, 180, 80);
            }
            else
            {
                SyncFoodRainState();
            }
        }

        public override void PostUpdateEverything()
        {
            // 多人客户端不负责推进事件逻辑，只负责显示同步后的UI
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            bool stateChanged = false;

            if (Active)
            {
                if (Owner < 0 || Owner >= Main.maxPlayers)
                {
                    Active = false;
                    TimeLeft = 0;
                    PostWinDelay = 0;
                    Owner = -1;
                    stateChanged = true;
                }
                else
                {
                    Player p = Main.player[Owner];

                    if (!p.active || p.dead)
                    {
                        Active = false;
                        TimeLeft = 0;
                        PostWinDelay = 0;
                        Owner = -1;
                        stateChanged = true;
                    }
                    else if (TimeLeft > 0)
                    {
                        TimeLeft--;

                        // 生成射弹：只在服务器/单机
                        if (Main.GameUpdateCount % 12 == 0)
                            SpawnOneWave(p);

                        if (TimeLeft <= 0)
                        {
                            Active = false;
                            TimeLeft = 0;
                            PostWinDelay = 60 * 10; // 10秒后召唤食神
                            stateChanged = true;

                            if (Main.netMode == NetmodeID.SinglePlayer)
                                Main.NewText(Language.GetTextValue("Mods.WuDao.Events.FoodRain.Win"), 255, 220, 120);
                        }
                    }
                }
            }

            if (!Active && PostWinDelay > 0)
            {
                if (Owner < 0 || Owner >= Main.maxPlayers)
                {
                    PostWinDelay = 0;
                    Owner = -1;
                    stateChanged = true;
                }
                else
                {
                    Player p2 = Main.player[Owner];

                    if (!p2.active || p2.dead)
                    {
                        PostWinDelay = 0;
                        Owner = -1;
                        stateChanged = true;
                    }
                    else
                    {
                        PostWinDelay--;

                        if (PostWinDelay <= 0)
                        {
                            PostWinDelay = 0;

                            Vector2 pos = p2.Center + new Vector2(0f, -600f);
                            NPC.NewNPC(
                                p2.GetSource_Misc("FoodRainWin"),
                                (int)pos.X,
                                (int)pos.Y,
                                ModContent.NPCType<FoodGodBoss>()
                            );

                            Owner = -1;
                            stateChanged = true;
                        }
                    }
                }
            }

            // 周期同步倒计时给客户端，让UI能更新
            if (Main.netMode == NetmodeID.Server)
            {
                if (stateChanged || (Active || PostWinDelay > 0) && Main.GameUpdateCount % 30 == 0)
                    SyncFoodRainState();
            }
        }

        private static void SpawnOneWave(Player p)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int count = Main.rand.Next(2, 4);

            for (int i = 0; i < count; i++)
            {
                int foodItemId = Helpers.GetRandomFromSet(ItemID.Sets.IsFood);
                bool harmful = Main.rand.NextBool();

                Vector2 spawn = p.Center + new Vector2(Main.rand.NextFloat(-800f, 800f), -600f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2.1f, 2.1f), Main.rand.NextFloat(3.4f, 6.8f));

                int proj = Projectile.NewProjectile(
                    p.GetSource_Misc("FoodRain"),
                    spawn,
                    vel,
                    ModContent.ProjectileType<FoodRainProjectile>(),
                    harmful ? 5 : 0,
                    0f,
                    Owner,
                    harmful ? 1f : 0f,
                    foodItemId
                );

                if (proj >= 0 && proj < Main.maxProjectiles)
                    Main.projectile[proj].netUpdate = true;
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
                title = Language.GetTextValue("Mods.WuDao.Events.FoodRain.UI.Title");
                sub = Language.GetTextValue("Mods.WuDao.Events.FoodRain.UI.Progress", (int)(percent * 100), FormatTime(TimeLeft));
            }
            else
            {
                percent = MathHelper.Clamp(1f - PostWinDelay / 600f, 0f, 1f);
                title = Language.GetTextValue("Mods.WuDao.Events.FoodRain.UI.WinTitle");
                sub = Language.GetTextValue("Mods.WuDao.Events.FoodRain.UI.SummonCountdown", PostWinDelay / 60f);
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
        private static void SyncFoodRainState()
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }
    }
}
