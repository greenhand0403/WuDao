// Systems/WishingWellSystem.cs
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Tiles;

namespace WuDao.Content.Systems
{
    public class WishingWellSystem : ModSystem
    {
        // 对外字典（跨模组也会用到）
        internal static readonly Dictionary<int, int> BagToBoss = new();   // 宝藏袋 → Boss
        internal static readonly Dictionary<int, int> ItemToBoss = new();  // 召唤物/掉落物 → Boss
        internal static readonly Dictionary<int, Func<Player, bool>> BossEnvOk = new(); // 环境判定

        public override void OnWorldLoad()
        {
            BagToBoss.Clear();
            ItemToBoss.Clear();
            BossEnvOk.Clear();

            // ====== 宝藏袋 → Boss（全原版）======
            BagToBoss[ItemID.KingSlimeBossBag] = NPCID.KingSlime;
            BagToBoss[ItemID.EyeOfCthulhuBossBag] = NPCID.EyeofCthulhu;
            BagToBoss[ItemID.EaterOfWorldsBossBag] = NPCID.EaterofWorldsHead;
            BagToBoss[ItemID.BrainOfCthulhuBossBag] = NPCID.BrainofCthulhu;
            BagToBoss[ItemID.QueenBeeBossBag] = NPCID.QueenBee;
            BagToBoss[ItemID.SkeletronBossBag] = NPCID.SkeletronHead;
            BagToBoss[ItemID.DeerclopsBossBag] = NPCID.Deerclops;
            BagToBoss[ItemID.WallOfFleshBossBag] = NPCID.WallofFlesh;

            BagToBoss[ItemID.QueenSlimeBossBag] = NPCID.QueenSlimeBoss;
            BagToBoss[ItemID.DestroyerBossBag] = NPCID.TheDestroyer;
            BagToBoss[ItemID.TwinsBossBag] = NPCID.Retinazer; // Twins 特判一起召唤
            BagToBoss[ItemID.SkeletronPrimeBossBag] = NPCID.SkeletronPrime;
            BagToBoss[ItemID.PlanteraBossBag] = NPCID.Plantera;
            BagToBoss[ItemID.GolemBossBag] = NPCID.Golem;
            BagToBoss[ItemID.FishronBossBag] = NPCID.DukeFishron;
            BagToBoss[ItemID.FairyQueenBossBag] = NPCID.HallowBoss; // 光之女皇
            BagToBoss[ItemID.CultistBossBag] = NPCID.CultistBoss;
            BagToBoss[ItemID.MoonLordBossBag] = NPCID.MoonLordCore;

            // ====== 召唤物 / 经典掉落 → Boss（常用项，够用即可，后续你可继续补）======
            // 召唤物
            ItemToBoss[ItemID.SlimeCrown] = NPCID.KingSlime;
            ItemToBoss[ItemID.SuspiciousLookingEye] = NPCID.EyeofCthulhu;
            ItemToBoss[ItemID.WormFood] = NPCID.EaterofWorldsHead;
            ItemToBoss[ItemID.BloodySpine] = NPCID.BrainofCthulhu;
            ItemToBoss[ItemID.Abeemination] = NPCID.QueenBee;
            ItemToBoss[ItemID.DeerThing] = NPCID.Deerclops;
            ItemToBoss[ItemID.GuideVoodooDoll] = NPCID.WallofFlesh;         // 扔岩浆触发；在许愿池里当作“关联物”
            ItemToBoss[ItemID.VolatileGelatin] = NPCID.QueenSlimeBoss;      // 地下神圣召唤
            ItemToBoss[ItemID.MechanicalWorm] = NPCID.TheDestroyer;
            ItemToBoss[ItemID.MechanicalEye] = NPCID.Retinazer;           // Twins
            ItemToBoss[ItemID.MechanicalSkull] = NPCID.SkeletronPrime;
            ItemToBoss[ItemID.LihzahrdPowerCell] = NPCID.Golem;               // 神庙电池
            ItemToBoss[4961] = NPCID.HallowBoss;          // 夜晚圣地击杀触发，这里作为“关联物”
            ItemToBoss[ItemID.CelestialSigil] = NPCID.MoonLordCore;

            // 代表性掉落（部分举例，满足“BOSS 掉落物也能投”的需求）
            ItemToBoss[ItemID.SlimySaddle] = NPCID.KingSlime;
            ItemToBoss[3097] = NPCID.EyeofCthulhu;
            ItemToBoss[ItemID.WormScarf] = NPCID.EaterofWorldsHead;
            ItemToBoss[ItemID.BrainOfConfusion] = NPCID.BrainofCthulhu;
            ItemToBoss[ItemID.BeeGun] = NPCID.QueenBee;
            ItemToBoss[ItemID.BeeKeeper] = NPCID.QueenBee;
            ItemToBoss[ItemID.BookofSkulls] = NPCID.SkeletronHead;
            ItemToBoss[ItemID.SkeletronHand] = NPCID.SkeletronHead;
            ItemToBoss[ItemID.LucyTheAxe] = NPCID.Deerclops;
            ItemToBoss[ItemID.Pwnhammer] = NPCID.WallofFlesh;

            ItemToBoss[ItemID.QueenSlimeMountSaddle] = NPCID.QueenSlimeBoss;      // Gelatinous Pillion
            ItemToBoss[ItemID.SoulofMight] = NPCID.TheDestroyer;
            ItemToBoss[ItemID.SoulofSight] = NPCID.Retinazer;
            ItemToBoss[ItemID.SoulofFright] = NPCID.SkeletronPrime;
            ItemToBoss[ItemID.PygmyStaff] = NPCID.Plantera;
            ItemToBoss[ItemID.Seedler] = NPCID.Plantera;
            ItemToBoss[ItemID.Picksaw] = NPCID.Golem;
            ItemToBoss[ItemID.EyeoftheGolem] = NPCID.Golem;
            ItemToBoss[ItemID.RazorbladeTyphoon] = NPCID.DukeFishron;
            ItemToBoss[ItemID.Flairon] = NPCID.DukeFishron;
            ItemToBoss[4914] = NPCID.HallowBoss;
            ItemToBoss[4952] = NPCID.HallowBoss;
            ItemToBoss[3549] = NPCID.CultistBoss;
            ItemToBoss[ItemID.LastPrism] = NPCID.MoonLordCore;
            ItemToBoss[ItemID.Terrarian] = NPCID.MoonLordCore;
            ItemToBoss[ItemID.Meowmere] = NPCID.MoonLordCore;
            ItemToBoss[ItemID.StarWrath] = NPCID.MoonLordCore;

            // ====== 环境判定（尽量贴近原版；为避免 API 差异，做“宽松版”）======
            BossEnvOk[NPCID.KingSlime] = p => true;
            BossEnvOk[NPCID.EyeofCthulhu] = p => !Main.dayTime && p.ZoneOverworldHeight;
            BossEnvOk[NPCID.EaterofWorldsHead] = p => p.ZoneCorrupt;
            BossEnvOk[NPCID.BrainofCthulhu] = p => p.ZoneCrimson;
            BossEnvOk[NPCID.QueenBee] = p => p.ZoneJungle;
            BossEnvOk[NPCID.SkeletronHead] = p => !Main.dayTime;                // 原版需在地表地牢前，但这里放宽为夜间
            BossEnvOk[NPCID.Deerclops] = p => p.ZoneSnow;                    // 放宽为雪原即可
            BossEnvOk[NPCID.WallofFlesh] = p => p.ZoneUnderworldHeight;

            BossEnvOk[NPCID.QueenSlimeBoss] = p => Main.hardMode && p.ZoneHallow;// 宽松：在神圣任意层
            BossEnvOk[NPCID.TheDestroyer] = p => Main.hardMode && !Main.dayTime;
            BossEnvOk[NPCID.Retinazer] = p => Main.hardMode && !Main.dayTime;
            BossEnvOk[NPCID.SkeletronPrime] = p => Main.hardMode && !Main.dayTime;
            BossEnvOk[NPCID.Plantera] = p => NPC.downedMechBossAny && p.ZoneJungle;
            BossEnvOk[NPCID.Golem] = p => NPC.downedPlantBoss;          // 放宽：不强制神庙区域以减少 API 兼容风险
            BossEnvOk[NPCID.DukeFishron] = p => p.ZoneBeach;
            BossEnvOk[NPCID.HallowBoss] = p => p.ZoneHallow && !Main.dayTime;// 光之女皇：夜间圣地
            BossEnvOk[NPCID.CultistBoss] = p => NPC.downedGolemBoss;          // 放宽：不强制地牢坐标
            BossEnvOk[NPCID.MoonLordCore] = p => NPC.downedAncientCultist;     // 击败教徒后
        }

        // ===== 世界生成：出生点下方“洞穴层”随机生成许愿池，并在左右各放生命水晶 =====
        public override void PostWorldGen()
        {
            try
            {
                int spawnX = Main.spawnTileX;
                int x = spawnX + WorldGen.genRand.Next(-80, 81);
                int yStart = (int)(Main.rockLayer) + WorldGen.genRand.Next(30, 120);
                yStart = Math.Clamp(yStart, 0, Main.maxTilesY - 50);

                Point? found = null;
                for (int y = yStart; y < Math.Min(yStart + 300, Main.maxTilesY - 40); y++)
                {
                    if (IsRoomFor3x3(x, y) && SolidBelow(x, y + 2))
                    {
                        found = new Point(x, y);
                        break;
                    }
                }
                if (!found.HasValue) return;

                var p = found.Value;
                WorldGen.PlaceObject(p.X, p.Y, ModContent.TileType<WishingWellTile>());
                // 打印生成的位置坐标
                Main.NewText($"许愿池生成在：X={p.X}, Y={p.Y}");
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendObjectPlacement(-1, p.X, p.Y, ModContent.TileType<WishingWellTile>(), 0, 0, -1, -1);

                TryPlaceLifeCrystalNear(new Point(p.X - 5, p.Y));
                TryPlaceLifeCrystalNear(new Point(p.X + 5, p.Y));
            }
            catch { }
        }

        private static bool IsRoomFor3x3(int i, int j)
        {
            for (int x = i; x < i + 3; x++)
                for (int y = j; y < j + 3; y++)
                    if (!WorldGen.InWorld(x, y) || Main.tile[x, y].HasTile) return false;
            return true;
        }
        private static bool SolidBelow(int i, int j)
            => WorldGen.InWorld(i, j + 1) && Main.tile[i, j + 1].HasTile && Main.tileSolid[Main.tile[i, j + 1].TileType];

        private static void TryPlaceLifeCrystalNear(Point approx)
        {
            for (int dy = 0; dy < 30; dy++)
            {
                int x = approx.X;
                int y = approx.Y + dy;
                if (!WorldGen.InWorld(x, y + 1)) break;
                if (!Main.tile[x, y].HasTile && Main.tile[x, y + 1].HasTile && Main.tileSolid[Main.tile[x, y + 1].TileType])
                {
                    WorldGen.PlaceObject(x, y, TileID.Heart);
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendObjectPlacement(-1, x, y, TileID.Heart, 0, 0, -1, -1);
                    break;
                }
            }
        }

        // ===== 公共工具 =====
        internal static bool TryResolveBossFromItem(Item item, out int bossID)
        {
            bossID = -1;
            int type = item.type;

            // 1) 宝藏袋：先用内置标志，再查映射
            if (type < ItemID.Count && ItemID.Sets.BossBag[type] && BagToBoss.TryGetValue(type, out bossID))
                return true;

            // 2) 召唤物 / 代表性掉落
            if (ItemToBoss.TryGetValue(type, out bossID))
                return true;

            return false;
        }

        internal static bool CheckEnvOk(int bossID, Player p)
        {
            if (BossEnvOk.TryGetValue(bossID, out var cond))
                return cond?.Invoke(p) ?? true;
            return true;
        }

        internal static void SpawnBoss(Player p, int bossID)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Twins：两只一起
            if (bossID == NPCID.Retinazer)
            {
                NPC.SpawnOnPlayer(p.whoAmI, NPCID.Spazmatism);
                NPC.SpawnOnPlayer(p.whoAmI, NPCID.Retinazer);
                return;
            }

            NPC.SpawnOnPlayer(p.whoAmI, bossID);
        }
    }
}
