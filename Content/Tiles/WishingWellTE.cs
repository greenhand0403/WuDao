// Content/Tiles/WishingWellTE.cs
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Config;
using WuDao.Systems;

namespace WuDao.Content.Tiles
{
    public class WishingWellTE : ModTileEntity
    {
        // ✅ 新增：让 Tile 能读到“是否可用”
        public bool IsReady => _cooldown <= 0;
        // 新增：每次触发后进入冷却（单位：帧）
        private int _cooldown = 0;
        public override void Update()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            // 冷却递减
            if (_cooldown > 0)
            {
                _cooldown--;
                return; // 冷却时不处理任何物品
            }
            Rectangle area = GetTouchArea();
            int plr = Player.FindClosest(Position.ToWorldCoordinates(24, 24), 1, 1);
            Player player = Main.player[plr];

            for (int i = 0; i < Main.maxItems; i++)
            {
                Item it = Main.item[i];
                if (!it.active || it.stack <= 0) continue;

                if (!area.Intersects(it.Hitbox))
                    continue;

                // 仅处理“BOSS 关联物”
                if (!WishingWellSystem.TryResolveBossFromItem(it, out int bossID))
                {
                    CombatText.NewText(player.Hitbox, Color.Gray, $"许愿井对它不感兴趣");
                    continue;
                }

                // 环境不满足：完全不响应（也不吞）
                if (!WishingWellSystem.CheckEnvOk(bossID, player))
                {
                    CombatText.NewText(player.Hitbox, Color.Gray, $"BOSS生成环境不满足");
                    continue;
                }

                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i);

                // 概率决定吐双倍 or 召唤
                float chance = ModContent.GetInstance<WishingWellConfig>().Chance;
                bool giveDouble = Main.rand.NextFloat() < chance;
                int r = Math.Min(it.stack, Main.rand.Next(2, 6));

                if (giveDouble)
                {
                    CombatText.NewText(player.Hitbox, Color.Yellow, $"额外奖励{r}个");
                    player.QuickSpawnItem(player.GetSource_GiftOrReward(), it.type, r);
                    for (int d = 0; d < 18; d++)
                    {
                        int dust = Dust.NewDust(Position.ToWorldCoordinates(16, 8), 32, 16, DustID.Firework_Red);
                        Main.dust[dust].velocity *= 0.4f;
                    }
                }
                else
                {
                    CombatText.NewText(player.Hitbox, Color.Yellow, $"运气不佳");
                    for (int bossAmount = 0; bossAmount < r; bossAmount++)
                    {
                        WishingWellSystem.SpawnBoss(player, bossID);
                    }
                    for (int d = 0; d < 24; d++)
                    {
                        int dust = Dust.NewDust(Position.ToWorldCoordinates(16, 8), 32, 16, DustID.Firework_Green);
                        Main.dust[dust].velocity *= 0.6f;
                    }
                    // 失败吞 1 个
                    it.stack -= r;
                    if (it.stack <= 0) it.TurnToAir();
                }

                // 关键：处理完一个物品就进入冷却
                _cooldown = 300; // 300 帧≈5秒；需要更慢就加大
                break; // 一次只吃一个
            }
        }

        private Rectangle GetTouchArea()
        {
            // 井口上方一个小判定区
            var world = Position.ToWorldCoordinates(0, 0);
            return new Rectangle((int)world.X + 8, (int)world.Y - 8, 48, 24);
        }

        public override bool IsTileValidForEntity(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return t.HasTile && t.TileType == ModContent.TileType<WishingWellTile>();
        }
    }
}
