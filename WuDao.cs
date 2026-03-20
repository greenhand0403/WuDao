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
		SyncLifePenalty,
		SyncJuexueSlot,
		SelectBundleCategory,
		SyncSheRaTransform,
		RequestTimeStop,
		SyncTimeStopState
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

				case MessageType.SyncJuexueSlot:
					{
						byte plr = reader.ReadByte();

						if (plr < 0 || plr >= Main.maxPlayers)
							return;

						Player player = Main.player[plr];
						if (player == null || !player.active)
							return;

						QiPlayer qi = player.GetModPlayer<QiPlayer>();
						qi.JuexueSlot = QiPlayer.ReadSimpleItem(reader);

						// 客户端 -> 服务器：服务器收到后转发给其他客户端
						if (Main.netMode == NetmodeID.Server)
						{
							ModPacket packet = GetPacket();
							packet.Write((byte)MessageType.SyncJuexueSlot);
							packet.Write(plr);
							QiPlayer.WriteSimpleItem(packet, qi.JuexueSlot);
							packet.Send(-1, whoAmI);
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
