using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Global.NPCs;

namespace WuDao.Content.Systems
{
    // 出售绝学的 NPC：流浪乞丐
    public class BeggarSystem : ModSystem
    {
        private bool lastDay = true;

        public override void PostUpdateWorld()
        {
            // ✅ 只让服务器/单机端决定生成，避免多人每个客户端都刷一次
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // 早晨切换检测
            if (Main.dayTime && !lastDay)
            {
                OnNewDay();
            }
            lastDay = Main.dayTime;
        }

        private void OnNewDay()
        {
            // 如果已经存在则不生成
            if (NPC.AnyNPCs(ModContent.NPCType<WanderingBeggar>()))
                return;

            // 5% 几率在白天到来时生成一个流浪乞丐 1/20
            if (Main.rand.NextBool(20))
            {
                SpawnBeggarNearTown();
            }
        }

        public static void SpawnBeggarNearTown()
        {
            // 选一个“城镇NPC”作为中心点（像旅商：靠近城镇）
            int anchorIndex = FindTownAnchorNPCIndex();

            // 如果城镇里一个NPC都没有，就退化为在任意活跃玩家附近刷（兜底）
            Vector2 anchorWorldPos;
            if (anchorIndex != -1)
            {
                anchorWorldPos = Main.npc[anchorIndex].Bottom;
            }
            else
            {
                Player p = FindAnyActivePlayer();
                if (p == null) return;
                anchorWorldPos = p.Bottom;
            }

            // 尝试在城镇NPC附近找一个可站立的落点
            if (!TryFindStandableSpotNear(anchorWorldPos, out Vector2 spawnPos))
            {
                // 找不到就稍微偏移一下兜底
                spawnPos = anchorWorldPos + new Vector2(120f, -16f);
            }

            int id = NPC.NewNPC(
                Main.LocalPlayer.GetSource_Misc("WanderingBeggarSummon"),
                (int)spawnPos.X,
                (int)spawnPos.Y,
                ModContent.NPCType<WanderingBeggar>()
            );

            if (id >= 0 && id < Main.maxNPCs)
            {
                Main.npc[id].netUpdate = true;
            }

            Main.NewText(Language.GetTextValue("Mods.WuDao.Items.SteamedBun.Messages"), Color.LightGreen);
        }

        private static int FindTownAnchorNPCIndex()
        {
            // 收集可用的城镇NPC（活跃、townNPC）
            int[] candidates = new int[Main.maxNPCs];
            int count = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active) continue;

                // ✅ townNPC：真正城镇NPC
                if (!n.townNPC) continue;

                // 过滤一些特殊情况（可按你需求增减）
                if (n.type == NPCID.OldMan) continue;

                candidates[count++] = i;
            }

            if (count == 0)
                return -1;

            return candidates[Main.rand.Next(count)];
        }

        private static Player FindAnyActivePlayer()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p != null && p.active)
                    return p;
            }
            return null;
        }

        /// <summary>
        /// 在 anchorWorldPos 附近找一个“可站立”的点：头顶2格空、脚下是实心块、不是熔岩。
        /// </summary>
        private static bool TryFindStandableSpotNear(Vector2 anchorWorldPos, out Vector2 worldSpawnPos)
        {
            worldSpawnPos = default;

            int baseX = (int)(anchorWorldPos.X / 16f);
            int baseY = (int)(anchorWorldPos.Y / 16f);

            // 搜索范围：水平约 60 格，垂直上下小范围，向下找地面
            const int tries = 200;
            const int rangeX = 60;
            const int rangeY = 12;
            const int searchDown = 30;

            for (int t = 0; t < tries; t++)
            {
                int x = baseX + Main.rand.Next(-rangeX, rangeX + 1);
                int y = baseY + Main.rand.Next(-rangeY, rangeY + 1);

                if (!WorldGen.InWorld(x, y, 40))
                    continue;

                // 向下找地面
                int groundY = -1;
                for (int dy = 0; dy <= searchDown; dy++)
                {
                    int yy = y + dy;
                    if (!WorldGen.InWorld(x, yy, 40))
                        break;

                    Tile tile = Main.tile[x, yy];
                    if (tile == null) continue;

                    // 找到实心地面
                    if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        groundY = yy;
                        break;
                    }
                }

                if (groundY == -1)
                    continue;

                // 站立点：地面上一格
                int standY = groundY - 1;

                // 检查站立空间（站立格 + 头顶一格）必须为空
                if (!IsAir(x, standY) || !IsAir(x, standY - 1))
                    continue;

                // 避免熔岩/蜂蜜/水深等（这里先防熔岩即可）
                if (Main.tile[x, groundY].LiquidAmount > 0 && Main.tile[x, groundY].LiquidType == LiquidID.Lava)
                    continue;

                // 转成世界坐标（让脚落在地面上方）
                worldSpawnPos = new Vector2(x * 16 + 8, standY * 16);
                return true;
            }

            return false;
        }

        private static bool IsAir(int x, int y)
        {
            if (!WorldGen.InWorld(x, y, 40))
                return false;

            Tile tile = Main.tile[x, y];
            if (tile == null) return false;

            return !tile.HasTile && tile.LiquidAmount == 0;
        }

        internal static void SpawnBeggarNear(Player player)
        {
            throw new NotImplementedException();
        }
    }
}
