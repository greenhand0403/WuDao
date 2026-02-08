//#r "WuDao"
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Reflection;
using Microsoft.Xna.Framework;                 // Color
using Microsoft.Xna.Framework.Graphics;       // Texture2D
using ReLogic.Content;                        // Asset, AssetRequestMode
using Terraria.GameContent;
// Modders toolkit v0.21
// MacOS path: /Users/unkvcc/Library/Application Support/Terraria/tModLoader/Mods/Cache/ModdersToolkit_Code.cs
// C# REPL run:
// T.R()
public class T
{
    public const string buffName = "BarrenLand";
    public static void R(int variant = 2)
    {
        // 1) 取得玩家（REPL 在客户端界面执行时有 LocalPlayer；专服控制台执行时通常没有）
        Player player = Main.LocalPlayer;
        if (player is null)
        {
            Main.NewText("找不到 LocalPlayer（是不是在主菜单/服务器控制台执行？）", Color.Red);
            return;
        }

        // 2) 取得 WuDao 模组实例（注意：这里用的是“模组内部名字”，不等于 DLL 文件名时可用下面的枚举确认）
        if (!ModLoader.TryGetMod("WuDao", out Mod wuDao))
        {
            Main.NewText("未找到已加载的模组：WuDao", Color.Red);

            //（可选）枚举当前已加载模组的内部名，帮你确认正确名字
            foreach (var m in ModLoader.Mods)
                Main.NewText($"Loaded Mod: {m.Name}", Color.Yellow);
            return;
        }
        #region buff
        // 3) 在该模组里按“内容名”查找 Buff —— 不需要知道命名空间
        if (!wuDao.TryFind<ModBuff>(buffName, out var buff))
        {
            Main.NewText($"WuDao 中找不到 Buff 名为 \"{buffName}\" 的内容。可用 Buff 列表如下：", Color.Orange);

            //（可选）把这个模组里所有 Buff 的完整名打印出来，方便你确认正确叫法
            foreach (var b in wuDao.GetContent<ModBuff>())
                Main.NewText(" - " + b.FullName, Color.LightGray);
            return;
        }

        // 4) 加 Buff（240 tick = 10 秒）
        player.AddBuff(buff.Type, 600);

        // 5) 验证
        // Main.NewText($"已应用 {buff.FullName} ：{player.HasBuff(buff.Type)}", Color.LimeGreen);

        // 3) 在该模组里按“内容名”查找 Buff —— 不需要知道命名空间
        // if (!wuDao.TryFind<ModPlayer>("BuffPlayer", out var buffp))
        // {
        //     Main.NewText($"WuDao 中找不到 Buff 名为 \"buffp\" 的内容。可用 Buff 列表如下：", Color.Orange);
        //     return;
        // }

        // 5) 验证
        // Main.NewText($"已应用buffp", Color.LimeGreen);
        #endregion
        #region 刀光贴图 获取类 + 修改静态字段
        // 1) 获取类型：WuDao.Content.Projectiles.Melee.ScallionSwordProj
        var type = wuDao.Code.GetType("WuDao.Content.Projectiles.Melee.ScallionSwordProj");
        if (type == null) { Main.NewText("找不到类型 WuDao.Content.Projectiles.Melee.ScallionSwordProj", Color.Red); return; }

        // 2) 获取私有静态字段 TexAsset 报错：“无法访问静态字段”
        var texField = type.GetField("TexAsset", BindingFlags.Static | BindingFlags.NonPublic);
        if (texField == null) { Main.NewText("找不到静态字段 TexAsset", Color.Red); return; }

        // 3) 选择要载入的贴图路径
        string path = variant == 2
            ? "WuDao/Content/Projectiles/Melee/ScallionSwordTail2"
            : variant == 3
                ? "WuDao/Content/Projectiles/Melee/ScallionSwordTail3"
                : "WuDao/Content/Projectiles/Melee/ScallionSwordTail";

        // 4) 请求并立即加载（马上可用），然后写入静态字段
        var asset = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad);
        texField.SetValue(null, asset); // null 因为是静态字段

        Main.NewText($"ScallionSword Tail 切换到: {path}", Color.LimeGreen);

        // 更换弹幕贴图
        // var asset = ModContent.Request<Texture2D>("WuDao/Content/Items/Weapons/Melee/InvincibleBlade", AssetRequestMode.ImmediateLoad);
        // var asset = TextureAssets.Item[ItemID.TerraBlade];
        // texField.SetValue(null, asset); // null 因为是静态字段
        // if (!wuDao.TryFind<ModProjectile>("ScallionSwordProj", out var sproj))
        // {
        //     Main.NewText($"WuDao 中找不到 ScallionSwordProj", Color.Orange);
        //     return;
        // }
        // 加载新的贴图并更换原本的贴图
        // TextureAssets.Projectile[sproj.Type] = asset;

        #endregion
    }
}