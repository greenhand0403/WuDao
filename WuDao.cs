using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Systems;
using WuDao.Common.Rendering;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using WuDao.Content.Development;
using WuDao.Content.Global;

namespace WuDao
{
	// 染色刀光
	// 共享顶点缓冲，避免每个 proj 都 new List
	public static class BladeTrailScratchBuffer
	{
		public static readonly List<BladeTrailRenderer.V> Verts = new();
	}
	// 自动喝药 把消息枚举放到一个公共位置，避免到处重复定义
	public enum MessageType : byte
	{
		SyncLifePenalty,// 服用永生之酒减少生命上限，同步生命值惩罚
		SelectBundleCategory,// 同步开局礼包
		SyncSheRaTransform,// 同步希瑞之剑的变身
		RequestTimeStop,// 同步时间冻结道具的使用
		SyncTimeStopState,// 同步时间冻结状态
		SyncSkyWalkingState,// 同步月步绝学时玩家的状态
		SyncDesignFlawState,// 同步败笔状态
		SyncCuisineState,          // 同步厨艺/美味进度
		RequestCuisineCraftReward, // 客户端请求服务器发放菜谱双倍奖励
		RequestCuisineFoodRain,    // 客户端请求服务器尝试触发食物雨
		SyncMimickerState,         // 同步模仿者击杀/解锁进度
		SyncRewinderCicadasTriggered, // 同步春秋蝉触发视觉表现
		SyncOutlawPlayer,         // 同步法外狂徒状态
		SyncQiPermanentState,     // 同步气力上限恢复速度等增益永久状态
	}
	
	public class WuDao : Mod
	{
		public static Effect InvisibleSwordQiEffect;
        public override void Load()
		{
			if (!Main.dedServ)
				InvisibleSwordQiEffect = ModContent.Request<Effect>("WuDao/Effects/InvisibleSwordQi", AssetRequestMode.ImmediateLoad).Value;
        }
        public override void Unload()
        {
            InvisibleSwordQiEffect = null;
        }
		public void BroadcastTimeStopState(int toClient = -1, int ignoreClient = -1)
		{
			ModPacket packet = GetPacket();
			packet.Write((byte)MessageType.SyncTimeStopState);
			packet.Write(TimeStopSystem.IsFrozen);
			packet.Write(TimeStopSystem.Timer);
			packet.Write(TimeStopSystem.CooldownTimer);
			packet.Write((byte)TimeStopSystem.Scope);
			packet.Write((byte)(TimeStopSystem.AllowedPlayer < 0 ? 255 : TimeStopSystem.AllowedPlayer));
			packet.Send(toClient, ignoreClient);
		}
		public void BroadcastRewinderCicadasTriggered(int playerWhoAmI, int healedAmount, int toClient = -1, int ignoreClient = -1)
		{
			ModPacket packet = GetPacket();
			packet.Write((byte)MessageType.SyncRewinderCicadasTriggered);
			packet.Write((byte)playerWhoAmI);
			packet.Write(healedAmount);
			packet.Send(toClient, ignoreClient);
		}
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MessageType msg = (MessageType)reader.ReadByte();

			switch (msg)
			{
				case MessageType.SyncLifePenalty:
					{
						byte plr = reader.ReadByte();
						int penalty = reader.ReadInt32();

						if (plr >= 0 && plr < Main.maxPlayers)
						{
							Player player = Main.player[plr];
							if (player != null && player.active)
							{
								player.GetModPlayer<PotionPlayer>().maxLifePenalty = penalty;
							}
						}

						break;
					}

				case MessageType.SelectBundleCategory:
					{
						byte plr = reader.ReadByte();
						byte catRaw = reader.ReadByte();

						if (plr >= Main.maxPlayers)
							return;

						if (!System.Enum.IsDefined(typeof(BundleCategory), (int)catRaw))
							return;

						Player player = Main.player[plr];
						if (player == null || !player.active)
							return;

						if (Main.netMode == NetmodeID.Server)
						{
							BundleCategory category = (BundleCategory)catRaw;

							// 校验玩家身上是否真的持有礼包
							int itemType = ModContent.ItemType<WeaponBundleItem>();
							bool hasBundle = false;
							for (int i = 0; i < player.inventory.Length; i++)
							{
								if (player.inventory[i] != null && player.inventory[i].type == itemType && player.inventory[i].stack > 0)
								{
									hasBundle = true;
									break;
								}
							}

							if (!hasBundle)
								return;

							WeaponBundleItem.GiveItemsForCategoryNetSafe(player, category, this);
						}

						break;
					}
				case MessageType.SyncSheRaTransform:
					{
						byte plr = reader.ReadByte();
						bool transformed = reader.ReadBoolean();
						string setName = reader.ReadString();
						int timer = reader.ReadInt32();

						if (plr >= Main.maxPlayers)
							return;

						Player player = Main.player[plr];
						if (player == null || !player.active)
							return;

						var sheRaPlayer = player.GetModPlayer<SheRaSwordPlayer>();
						sheRaPlayer.SetTransformState(
							transformed,
							string.IsNullOrEmpty(setName) ? null : setName,
							timer
						);

						// 客户端 -> 服务器：服务器收到后转发给其他客户端
						if (Main.netMode == NetmodeID.Server)
						{
							ModPacket packet = GetPacket();
							packet.Write((byte)MessageType.SyncSheRaTransform);
							packet.Write(plr);
							packet.Write(transformed);
							packet.Write(setName ?? "");
							packet.Write(timer);
							packet.Send(-1, whoAmI);
						}

						break;
					}
				case MessageType.RequestTimeStop:
					{
						if (Main.netMode != NetmodeID.Server)
							return;

						FreezeScope scope = (FreezeScope)reader.ReadByte();
						int duration = reader.ReadInt32();
						int cooldown = reader.ReadInt32();
						byte allowedPlrRaw = reader.ReadByte();
						int allowedPlayer = allowedPlrRaw == 255 ? -1 : allowedPlrRaw;

						bool ok = TimeStopSystem.TryStartFreeze(duration, cooldown, scope, allowedPlayer);

						// 不管成功失败，都把当前状态广播出去，让客户端一致
						BroadcastTimeStopState();
						break;
					}

				case MessageType.SyncTimeStopState:
					{
						bool isFrozen = reader.ReadBoolean();
						int timer = reader.ReadInt32();
						int cooldownTimer = reader.ReadInt32();
						FreezeScope scope = (FreezeScope)reader.ReadByte();
						byte allowedPlrRaw = reader.ReadByte();
						int allowedPlayer = allowedPlrRaw == 255 ? -1 : allowedPlrRaw;

						TimeStopSystem.ApplySyncedState(isFrozen, timer, cooldownTimer, scope, allowedPlayer);
						break;
					}
				case MessageType.SyncSkyWalkingState:
					{
						byte playerId = reader.ReadByte();
						if (playerId >= Main.maxPlayers)
							return;

						Player player = Main.player[playerId];
						if (player == null || !player.active)
							return;
						QiPlayer qi = player.GetModPlayer<QiPlayer>();

						qi.SkyWalkingActive = reader.ReadBoolean();
						qi.SkyWalkingStandingOnAir = reader.ReadBoolean();
						qi.QiCurrent = reader.ReadSingle();

						if (Main.netMode == NetmodeID.Server)
						{
							// 转发给其他客户端
							ModPacket packet = GetPacket();
							packet.Write((byte)MessageType.SyncSkyWalkingState);
							packet.Write(playerId);
							packet.Write(qi.SkyWalkingActive);
							packet.Write(qi.SkyWalkingStandingOnAir);
							packet.Write(qi.QiCurrent);
							packet.Send(-1, whoAmI);
						}
						break;
					}
				case MessageType.SyncDesignFlawState:
					{
						byte playerId = reader.ReadByte();

						if (playerId >= Main.maxPlayers)
							return;

						Player player = Main.player[playerId];
						if (player == null || !player.active)
							return;

						DesignFlawPlayer flawPlayer = player.GetModPlayer<DesignFlawPlayer>();

						flawPlayer.recordedNPCType = reader.ReadInt32();
						flawPlayer.defeatCount = reader.ReadInt32();

						if (flawPlayer.defeatCount <= 0)
						{
							flawPlayer.recordedNPCType = -1;
							flawPlayer.defeatCount = 0;
						}

						if (Main.netMode == NetmodeID.Server)
						{
							ModPacket packet = GetPacket();
							packet.Write((byte)MessageType.SyncDesignFlawState);
							packet.Write(playerId);
							packet.Write(flawPlayer.recordedNPCType);
							packet.Write(flawPlayer.defeatCount);
							packet.Send(-1, whoAmI);
						}
						break;
					}
				case MessageType.SyncCuisineState:
					{
						byte playerId = reader.ReadByte();

						if (playerId >= Main.maxPlayers)
							return;

						Player player = Main.player[playerId];
						if (player == null || !player.active)
							return;

						CuisinePlayer cp = player.GetModPlayer<CuisinePlayer>();

						cp.CookingSkill = reader.ReadInt32();
						cp.Deliciousness = reader.ReadInt32();

						cp.CraftedEverFoods.Clear();
						int craftedEverCount = reader.ReadInt32();
						for (int i = 0; i < craftedEverCount; i++)
							cp.CraftedEverFoods.Add(reader.ReadInt32());

						cp.EatenEverFoods.Clear();
						int eatenEverCount = reader.ReadInt32();
						for (int i = 0; i < eatenEverCount; i++)
							cp.EatenEverFoods.Add(reader.ReadInt32());

						// 服务器收到客户端同步后，转发给其他客户端
						if (Main.netMode == NetmodeID.Server)
						{
							ModPacket packet = GetPacket();
							packet.Write((byte)MessageType.SyncCuisineState);
							packet.Write(playerId);
							packet.Write(cp.CookingSkill);
							packet.Write(cp.Deliciousness);

							packet.Write(cp.CraftedEverFoods.Count);
							foreach (int t in cp.CraftedEverFoods)
								packet.Write(t);

							packet.Write(cp.EatenEverFoods.Count);
							foreach (int t in cp.EatenEverFoods)
								packet.Write(t);

							packet.Send(-1, whoAmI);
						}
						break;
					}

				case MessageType.RequestCuisineCraftReward:
					{
						if (Main.netMode != NetmodeID.Server)
							return;

						byte playerId = reader.ReadByte();
						int itemType = reader.ReadInt32();
						int stack = reader.ReadInt32();

						if (playerId >= Main.maxPlayers || stack <= 0)
							return;

						Player player = Main.player[playerId];
						if (player == null || !player.active)
							return;

						player.QuickSpawnItem(player.GetSource_GiftOrReward(), itemType, stack);
						player.QuickSpawnItem(player.GetSource_GiftOrReward(), itemType, stack);
						break;
					}

				case MessageType.RequestCuisineFoodRain:
					{
						if (Main.netMode != NetmodeID.Server)
							return;

						byte playerId = reader.ReadByte();
						if (playerId >= Main.maxPlayers)
							return;

						Player player = Main.player[playerId];
						if (player == null || !player.active || player.dead)
							return;

						FoodRainSystem.TryTrigger(player);
						break;
					}
				case MessageType.SyncMimickerState:
					{
						byte playerId = reader.ReadByte();

						if (playerId >= Main.maxPlayers)
							return;

						Player player = Main.player[playerId];
						if (player == null || !player.active)
							return;

						MimickerPlayer mp = player.GetModPlayer<MimickerPlayer>();

						var progress = new Dictionary<int, int>();
						int progressCount = reader.ReadInt32();
						for (int i = 0; i < progressCount; i++)
						{
							int npcType = reader.ReadInt32();
							int kills = reader.ReadInt32();
							progress[npcType] = kills;
						}

						var unlocked = new HashSet<int>();
						int unlockedCount = reader.ReadInt32();
						for (int i = 0; i < unlockedCount; i++)
							unlocked.Add(reader.ReadInt32());

						mp.ApplySyncedState(progress, unlocked);
						break;
					}
				case MessageType.SyncRewinderCicadasTriggered:
					{
						byte plr = reader.ReadByte();
						int healedAmount = reader.ReadInt32();

						if (plr >= Main.maxPlayers)
							return;

						Player player = Main.player[plr];
						if (player == null || !player.active)
							return;

						// 服务器收到这种包时，不做任何事
						// 这个消息应当只由服务器发给客户端
						if (Main.netMode == NetmodeID.Server)
							return;

						// 客户端播放视觉效果
						Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.Center);

						CombatText.NewText(
							player.Hitbox,
							Microsoft.Xna.Framework.Color.Green,
							Terraria.Localization.Language.GetTextValue(
								"Mods.WuDao.Items.RewinderCicadas.Messages.Heal",
								healedAmount
							)
						);

						for (int i = 0; i < 25; i++)
						{
							int d = Dust.NewDust(player.position, player.width, player.height, DustID.MagicMirror);
							Main.dust[d].velocity *= 1.5f;
							Main.dust[d].noGravity = true;
						}

						break;
					}
				case MessageType.SyncOutlawPlayer:
					{
						byte plr = reader.ReadByte();
						Player player = Main.player[plr];
						var modPlayer = player.GetModPlayer<TheOutlawPlayer>();

						modPlayer.nextShotEmpowered = reader.ReadBoolean();
						modPlayer.critStacksForUltimate = reader.ReadInt32();
						modPlayer.ultimateReady = reader.ReadBoolean();
						modPlayer.dashBackCooldownTicks = reader.ReadInt32();

						if (Main.netMode == NetmodeID.Server)
						{
							ModPacket packet = GetPacket();
							packet.Write((byte)MessageType.SyncOutlawPlayer);
							packet.Write(plr);
							packet.Write(modPlayer.nextShotEmpowered);
							packet.Write(modPlayer.critStacksForUltimate);
							packet.Write(modPlayer.ultimateReady);
							packet.Write(modPlayer.dashBackCooldownTicks);
							packet.Send(-1, whoAmI);
						}
						break;
					}
				case MessageType.SyncQiPermanentState:
					{
						byte playerWhoAmI = reader.ReadByte();
						if (playerWhoAmI < 0 || playerWhoAmI >= Main.maxPlayers)
							return;

						Player player = Main.player[playerWhoAmI];
						QiPlayer qi = player.GetModPlayer<QiPlayer>();

						qi.QiMaxFromItems = reader.ReadInt32();
						qi.Used_ReiShi = reader.ReadInt32();
						qi.Used_PassionFruit = reader.ReadInt32();
						qi.JinggongUsed = reader.ReadInt32();
						qi.DonggongUsed = reader.ReadInt32();
						qi.QiRegenStandBonus = reader.ReadSingle();
						qi.QiRegenMoveBonus = reader.ReadSingle();

						if (Main.netMode == NetmodeID.Server)
						{
							ModPacket packet = GetPacket();
							packet.Write((byte)MessageType.SyncQiPermanentState);
							packet.Write(playerWhoAmI);

							packet.Write(qi.QiMaxFromItems);
							packet.Write(qi.Used_ReiShi);
							packet.Write(qi.Used_PassionFruit);
							packet.Write(qi.JinggongUsed);
							packet.Write(qi.DonggongUsed);
							packet.Write(qi.QiRegenStandBonus);
							packet.Write(qi.QiRegenMoveBonus);

							packet.Send(-1, whoAmI);
						}
						break;
					}
			}
		}
		// 厨艺和美味系统
		/// <summary>
		/// var wudao = ModLoader.GetMod("WuDao");
		/// <br>int made = (int)(wudao?.Call("GetCookbookMadeCount", Main.LocalPlayer) ?? 0);</br>
		/// <br>int eaten = (int)(wudao?.Call("GetFoodEatenCount", Main.LocalPlayer) ?? 0);</br>
		/// <br>Main.NewText($"[跨模组] 已制作={made}, 已品尝={eaten}");</br>
		/// </summary>
		public override object Call(params object[] args)
		{
			if (args is null || args.Length == 0 || args[0] is not string cmd) return null;

			Player p = Main.LocalPlayer;
			if (args.Length > 1 && args[1] is Player pp) p = pp;
			var cp = p?.GetModPlayer<CuisinePlayer>();

			return cmd switch
			{
				// 返回：已“制作过”的食物总数（来自菜谱池）
				"GetCookbookMadeCount" => (object)(cp?.CraftedFoodTypes.Count ?? 0),

				// 返回：全局“已吃过”的食物总数（含不可合成与模组）
				"GetFoodEatenCount" => (object)(cp?.FoodsEatenAll.Count ?? 0),

				// 返回：今日两道（int[2]，无则空数组）
				"GetTodayTwo" => (object)CuisineSystemGetTwo(p),
				// ★ 新增：注册“获取方式”提示（供你/他模组提前填无法合成的来源）
				// 用法：Mod.Call("CookbookRegisterHint", itemType, "由XXX掉落/购买/宝匣…")
				"CookbookRegisterHint" => (object)RegisterHint(args),
				"CookbookRegisterHintMany" => (object)RegisterHintManyCall(args),
				"GetCookingSkill" => (object)(cp?.CookingSkill ?? 0),
				"GetDeliciousness" => (object)(cp?.Deliciousness ?? 0),
				"RegisterWellBossBag" => (object)RegisterWellBossBag(args),
				"RegisterWellBossItem" => (object)RegisterWellBossItem(args),
				_ => null
			};
			/*
			// 本模组内
			var cp = Main.LocalPlayer.GetModPlayer<CuisinePlayer>();
			int cooking = cp.CookingSkill;
			int tasty   = cp.Deliciousness;

			// 跨模组
			var your = ModLoader.GetMod("YourModInternalName");
			int cooking2 = (int)(your?.Call("GetCookingSkill", Main.LocalPlayer) ?? 0);
			int tasty2   = (int)(your?.Call("GetDeliciousness", Main.LocalPlayer) ?? 0);
			*/
			static int[] CuisineSystemGetTwo(Player plr)
			{
				CuisineSystem.GetTodayTwo(plr, out int a, out int b);
				return (a > 0 || b > 0) ? new[] { a, b } : System.Array.Empty<int>();
			}
			static object RegisterHint(object[] argv)
			{
				if (argv.Length >= 3 && argv[1] is int type && argv[2] is string hint)
					CuisineSystem.ManualFoodHints[type] = hint;
				return null;
			}
			static object RegisterHintManyCall(object[] argv)
			{
				// 用法：Mod.Call("CookbookRegisterHintMany", new int[]{ItemID.Apple,ItemID.Apricot}, "摇晃森林的树木获得");
				if (argv.Length >= 3 && argv[1] is int[] arr && argv[2] is string hint)
					CuisineSystem.RegisterHintMany(arr, hint);
				return null;
			}
			// 许愿井
			static object RegisterWellBossBag(object[] args)
			{
				if (args.Length >= 3 && args[0] is string key && args[1] is int bossID && args[2] is int itemType)
					WishingWellSystem.BagToBoss[itemType] = bossID;
				return null;
			}
			static object RegisterWellBossItem(object[] args)
			{
				if (args.Length >= 3 && args[0] is string key && args[1] is int bossID && args[2] is int itemType)
					WishingWellSystem.ItemToBoss[itemType] = bossID;
				return null;
			}
		}

	}
}
