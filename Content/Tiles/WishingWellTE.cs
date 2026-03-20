using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Config;
using WuDao.Content.Systems;
using Terraria.Chat;
using Terraria.Localization;

namespace WuDao.Content.Tiles
{
    // 许愿井物块
    public class WishingWellTE : ModTileEntity
    {
        // Tile 用来读“是否可用”
        public bool IsReady => _cooldown <= 0;

        // 每次触发后进入冷却（单位：帧）
        private int _cooldown = 0;
        public override void Update()
        {
            // 多人客户端不处理真实逻辑，全部交给服务器/单机
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // 冷却递减
            if (_cooldown > 0)
            {
                _cooldown--;
                return;
            }

            Rectangle area = GetTouchArea();
            int plr = Player.FindClosest(Position.ToWorldCoordinates(24, 24), 1, 1);
            Player player = Main.player[plr];

            for (int i = 0; i < Main.maxItems; i++)
            {
                Item it = Main.item[i];
                if (!it.active || it.stack <= 0)
                    continue;

                if (!area.Intersects(it.Hitbox))
                    continue;

                if (!WishingWellSystem.TryResolveBossFromItem(it, out int bossID))
                {
                    BroadcastWellMessage("Mods.WuDao.WishingWellItem.Messages.ResolveBossFromItemFail", Color.Gray);
                    _cooldown = 30;
                    break;
                }

                if (!WishingWellSystem.CheckEnvOk(bossID, player))
                {
                    BroadcastWellMessage("Mods.WuDao.WishingWellItem.Messages.CheckEnvFail", Color.OrangeRed);
                    _cooldown = 30;
                    break;
                }

                // 服务器统一决定成功概率
                float chance = ModContent.GetInstance<WishingWellConfig>().Chance;
                bool giveDouble = Main.rand.NextFloat() < chance;

                // 本次处理数量
                int r = Math.Min(it.stack, Main.rand.Next(2, 6));

                if (giveDouble)
                {
                    // 成功：吐出额外奖励
                    player.QuickSpawnItem(player.GetSource_GiftOrReward(), it.type, r);

                    BroadcastWellMessage("Mods.WuDao.WishingWellItem.Messages.Success", Color.LightSkyBlue);
                }
                else
                {
                    // 失败：召唤 BOSS，并吞掉 r 个物品
                    for (int bossAmount = 0; bossAmount < r; bossAmount++)
                    {
                        WishingWellSystem.SpawnBoss(player, bossID);
                    }

                    it.stack -= r;
                    if (it.stack <= 0)
                        it.TurnToAir();

                    BroadcastWellMessage("Mods.WuDao.WishingWellItem.Messages.Fail", Color.IndianRed);
                }

                // 一定要在处理完成后再同步井口物品
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i);

                // 进入冷却
                _cooldown = 300;
                break; // 一次只处理一个物品
            }

            if (Main.netMode == NetmodeID.SinglePlayer)
                SpawnWellDustLocal();
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
        private static void BroadcastWellMessage(string key, Color color)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                ChatHelper.BroadcastChatMessage(
                    NetworkText.FromKey(key),
                    color
                );
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(Language.GetTextValue(key), color);
            }
        }

        private void SpawnWellDustLocal()
        {
            if (Main.dedServ)
                return;

            Vector2 world = Position.ToWorldCoordinates(24, 8);

            for (int k = 0; k < 16; k++)
            {
                int d = Dust.NewDust(world - new Vector2(12f, 12f), 24, 24, DustID.GoldFlame);
                Main.dust[d].velocity *= 1.4f;
                Main.dust[d].noGravity = true;
            }
        }
    }
}