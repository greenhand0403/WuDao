using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using WuDao.Common; // 这里放你已有的 SpriteSheet / SpriteAnimator
// 加载长精灵图
namespace WuDao.Common
{
    public enum SpriteAtlasId
    {
        Effect1,   // 你的 00.png/Effect1（纵向列）
        Effect2,
        BlueEffect,
        GreenEffect,
        PurpleEffect,
        RedEffect, // 你的 Red Effect 图集（多列不规则）
        YellowEffect
    }

    public static class SpriteSheets // 线程不涉及，这里简单做成静态
    {
        private static Dictionary<SpriteAtlasId, SpriteSheet> _map;

        public static bool Ready => _map != null;

        public static SpriteSheet Get(SpriteAtlasId id)
        {
            if (!Ready)
                Initialize(); // 保险：万一先调用了 Get 也能懒加载

            return _map[id];
        }

        public static void Initialize()
        {
            if (_map != null) return;

            _map = new Dictionary<SpriteAtlasId, SpriteSheet>();

            if (Main.dedServ)
            {
                // 服务器不加载贴图，但为了不空引用，仍放占位
                _map[SpriteAtlasId.Effect1] = SpriteSheet.FromTexture("WuDao/Assets/Effect1");
                _map[SpriteAtlasId.Effect2] = SpriteSheet.FromTexture("WuDao/Assets/Effect2");
                _map[SpriteAtlasId.BlueEffect] = SpriteSheet.FromTexture("WuDao/Assets/BlueEffect");
                _map[SpriteAtlasId.GreenEffect] = SpriteSheet.FromTexture("WuDao/Assets/GreenEffect");
                _map[SpriteAtlasId.PurpleEffect] = SpriteSheet.FromTexture("WuDao/Assets/PurpleEffect");
                _map[SpriteAtlasId.YellowEffect] = SpriteSheet.FromTexture("WuDao/Assets/YellowEffect");
                _map[SpriteAtlasId.RedEffect] = SpriteSheet.FromTexture("WuDao/Assets/RedEffect");
                return;
            }

            // ========= Effect1：纵向 N 列，列帧数不一致 =========
            // 举例：9列，每列帧数如下（按你的实际改）
            int[] framesPerCol = { 11, 10, 9, 9, 9, 9, 8, 7, 7 };
            var effect1 = SpriteSheet
                .FromTexture("WuDao/Assets/Effect1")
                .BuildVerticalColumns(
                    columns: framesPerCol.Length,
                    framesPerColumn: framesPerCol,
                    columnWidth: 64,
                    frameHeight: 64,
                    start: Point.Zero,
                    colSpacing: Point.Zero
                );
            _map[SpriteAtlasId.Effect1] = effect1;

            // ========= Effect2：纵向 N 列，列帧数一致 =========
            int[] framesPerCol2 = { 18, 15, 17 };
            var effect2 = SpriteSheet
                .FromTexture("WuDao/Assets/Effect2")
                .BuildVerticalColumns(
                    columns: 3,
                    framesPerColumn: framesPerCol2,
                    columnWidth: 64,
                    frameHeight: 64,
                    start: Point.Zero,
                    colSpacing: Point.Zero
                );
            _map[SpriteAtlasId.Effect2] = effect2;

            // ========= RedEffect：多列不规则，手工 AddSprite =========
            var red = SpriteSheet.FromTexture("WuDao/Assets/RedEffect")
                // 第1列一堆图标（示例，按你的坐标继续补）传入 起点坐标 帧步进 帧数
                // .AddSprite(new Rectangle(0, 0, 32, 32), new Point(0, 0), 1)
                // .AddSprite(new Rectangle(0, 96, 32, 32), new Point(0, 0), 1)
                // 第2列：有高矩形，横向3帧（示例坐标） 地狱之锋
                .AddSprite(new Rectangle(64, 256, 32, 64), new Point(32, 0), 3)
                // 法外狂徒 霰弹枪的火墙
                .AddSprite(new Rectangle(352, 384, 64, 32), new Point(64, 0), 2);

            _map[SpriteAtlasId.RedEffect] = red;

            // ========= BlueEffect：多列不规则，手工 AddSprite =========
            var blue = SpriteSheet.FromTexture("WuDao/Assets/BlueEffect")
                // 第1列一堆图标（示例，按你的坐标继续补）
                .AddSprite(new Rectangle(0, 0, 32, 32), new Point(0, 0), 1);
            // 继续 AddSprite(...) 把你需要用到的都登记完

            _map[SpriteAtlasId.BlueEffect] = blue;
            // ========= YellowEffect：多列不规则，手工 AddSprite =========
            var yellow = SpriteSheet.FromTexture("WuDao/Assets/YellowEffect")
                // 第1列一堆图标（示例，按你的坐标继续补）幽灵回旋镖
                .AddSprite(new Rectangle(0, 96, 32, 32), new Point(0, 0), 1);
            // 继续 AddSprite(...) 把你需要用到的都登记完

            _map[SpriteAtlasId.YellowEffect] = yellow;
            // ========= GreenEffect：多列不规则，手工 AddSprite =========
            var green = SpriteSheet.FromTexture("WuDao/Assets/GreenEffect")
                // 第1列一堆图标（示例，按你的坐标继续补）
                .AddSprite(new Rectangle(0, 0, 32, 32), new Point(0, 0), 1);
            // 继续 AddSprite(...) 把你需要用到的都登记完

            _map[SpriteAtlasId.GreenEffect] = green;
            // ========= PurpleEffect：多列不规则，手工 AddSprite =========
            var purple = SpriteSheet.FromTexture("WuDao/Assets/PurpleEffect")
                // 第1列一堆图标（示例，按你的坐标继续补）
                .AddSprite(new Rectangle(0, 0, 32, 32), new Point(0, 0), 1);
            // 继续 AddSprite(...) 把你需要用到的都登记完

            _map[SpriteAtlasId.PurpleEffect] = purple;
        }

        public static void Unload()
        {
            _map?.Clear();
            _map = null;
        }
    }

    public class SpriteSheetsSystem : ModSystem
    {
        public override void Load()
        {
            if (!Main.dedServ)
                SpriteSheets.Initialize();
        }

        public override void Unload()
        {
            SpriteSheets.Unload();
        }
    }
}
