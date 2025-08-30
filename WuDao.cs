using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using WuDao.Content.Players;
using WuDao.Content.Systems;
using WuDao.Systems;

namespace WuDao
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class WuDao : Mod
	{
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
