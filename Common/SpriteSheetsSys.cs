using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using WuDao.Common; // 这里放你已有的 SpriteSheet / SpriteAnimator

namespace WuDao.Common
{
    public enum SpriteAtlasId
    {
        Effect1,   // 你的 00.png/Effect1（纵向列）
        RedEffect, // 你的 Red Effect 图集（多列不规则）
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
                _map[SpriteAtlasId.RedEffect] = SpriteSheet.FromTexture("WuDao/Assets/RedEffect");
                return;
            }

            // ========= Effect1：纵向 N 列，列帧数不一致 =========
            // 举例：9列，每列帧数如下（按你的实际改）
            int[] framesPerCol = { 11, 8, 9, 7, 12, 6, 9, 8, 5 };
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

            // ========= RedEffect：多列不规则，手工 AddSprite =========
            var red = SpriteSheet.FromTexture("WuDao/Assets/RedEffect")
                // 第1列一堆图标（示例，按你的坐标继续补）
                .AddSprite(new Rectangle(0, 0, 32, 32), new Point(0, 0), 1)
                .AddSprite(new Rectangle(0, 32, 32, 32), new Point(0, 0), 1)
                // 第2列：有高矩形，横向3帧（示例坐标）
                .AddSprite(new Rectangle(64, 256, 32, 64), new Point(32, 0), 3);
            // 继续 AddSprite(...) 把你需要用到的都登记完

            _map[SpriteAtlasId.RedEffect] = red;
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
