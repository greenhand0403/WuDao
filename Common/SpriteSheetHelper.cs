using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent; // for TextureAssets if你想混用原版贴图

namespace WuDao.Common
{
    /// <summary>
    /// 描述一个精灵（由若干帧组成）的切割方式：给出第一帧矩形 + 帧数 + 每帧步进(dx,dy)。
    /// 例：横向3帧大小一致 -> firstFrame=(x,y,w,h), frameStep=(w,0), frameCount=3
    ///     纵向N帧 -> frameStep=(0,h)
    /// </summary>
    public struct SpriteDef
    {
        public Rectangle FirstFrame;
        public Point FrameStep;
        public int FrameCount;

        public SpriteDef(Rectangle firstFrame, Point frameStep, int frameCount)
        {
            FirstFrame = firstFrame;
            FrameStep = frameStep;
            FrameCount = Math.Max(1, frameCount);
        }

        public Rectangle GetFrameRect(int frameIndex)
        {
            frameIndex = ((frameIndex % FrameCount) + FrameCount) % FrameCount;
            return new Rectangle(
                FirstFrame.X + FrameStep.X * frameIndex,
                FirstFrame.Y + FrameStep.Y * frameIndex,
                FirstFrame.Width, FirstFrame.Height
            );
        }
    }

    /// <summary>
    /// 动画计时器（按tick切帧）
    /// </summary>
    public class SpriteAnimator
    {
        public int Frame;
        public int Counter;

        public void Update(int ticksPerFrame, int frameCount, bool loop = true)
        {
            if (ticksPerFrame <= 0 || frameCount <= 1)
                return;

            Counter++;
            if (Counter >= ticksPerFrame)
            {
                Counter = 0;
                Frame++;
                if (Frame >= frameCount)
                    Frame = loop ? 0 : frameCount - 1;
            }
        }

        public void Reset() { Frame = 0; Counter = 0; }
    }

    /// <summary>
    /// 通用 spritesheet 封装：支持规则网格、整列纵向、或手动不规则定义。
    /// </summary>
    public class SpriteSheet
    {
        public Asset<Texture2D> TextureAsset { get; private set; }
        public Texture2D Texture => TextureAsset.Value;
        public List<SpriteDef> Sprites { get; private set; } = new List<SpriteDef>();

        private SpriteSheet() { }

        public static SpriteSheet FromTexture(string assetPath)
        {
            return new SpriteSheet { TextureAsset = ModContent.Request<Texture2D>(assetPath) };
        }

        /// <summary>
        /// 规则网格切割（统一帧宽高）。可以产生许多“单帧精灵”或“多帧精灵”（按行/列组合）。
        /// </summary>
        public SpriteSheet BuildGrid(Rectangle sheetArea, Point frameSize, Point spacing, Point originOffset, Point gridCount, int framesPerSprite = 1, bool framesHorizontal = true)
        {
            // sheetArea: 要切的总区域（通常=整图 new Rectangle(0,0,texW,texH)）
            // frameSize: 单帧宽高
            // spacing:   相邻帧之间空隙（大多数为 Point.Zero）
            // originOffset: 第一个格子的起始偏移
            // gridCount: 横向、纵向一共多少格
            // framesPerSprite: 每个精灵包含多少帧（>=1）
            // framesHorizontal: 帧在每个精灵中是横向排还是纵向排

            Sprites.Clear();

            int texW = sheetArea.Width;
            int texH = sheetArea.Height;
            int startX = sheetArea.X + originOffset.X;
            int startY = sheetArea.Y + originOffset.Y;

            int stepX = frameSize.X + spacing.X;
            int stepY = frameSize.Y + spacing.Y;

            for (int gy = 0; gy < gridCount.Y; gy++)
            {
                for (int gx = 0; gx < gridCount.X; gx++)
                {
                    int x = startX + gx * stepX;
                    int y = startY + gy * stepY;

                    var first = new Rectangle(x, y, frameSize.X, frameSize.Y);
                    var step = framesHorizontal ? new Point(frameSize.X + spacing.X, 0)
                                                : new Point(0, frameSize.Y + spacing.Y);

                    Sprites.Add(new SpriteDef(first, step, Math.Max(1, framesPerSprite)));
                }
            }

            return this;
        }

        /// <summary>
        /// 整列纵向帧（适合你的 00.png：N列，每列一个精灵，纵向多帧）
        /// </summary>
        public SpriteSheet BuildVerticalColumns(int columns, int rowsPerColumn, bool equalCellHeight, int columnWidth, int cellHeight, Point start, Point colSpacing, int textureHeight)
        {
            // equalCellHeight=true 时，每格高度=cellHeight；否则按 rowsPerColumn 均分 textureHeight（常用：整列均分）
            Sprites.Clear();

            for (int c = 0; c < columns; c++)
            {
                int colX = start.X + c * (columnWidth + colSpacing.X);
                int colY = start.Y;

                Rectangle first;
                Point step;

                if (equalCellHeight)
                {
                    first = new Rectangle(colX, colY, columnWidth, cellHeight);
                    step = new Point(0, cellHeight + colSpacing.Y);
                }
                else
                {
                    int calcCell = textureHeight / rowsPerColumn; // 简单平均
                    first = new Rectangle(colX, colY, columnWidth, calcCell);
                    step = new Point(0, calcCell + colSpacing.Y);
                }

                Sprites.Add(new SpriteDef(first, step, rowsPerColumn));
            }

            return this;
        }
        // 在 SpriteSheet 内新增这个重载（仍在 YourMod.Common.Drawing 命名空间里）
        public SpriteSheet BuildVerticalColumns(
            int columns,
            int[] framesPerColumn,
            int columnWidth,
            int frameHeight,
            Point start,
            Point colSpacing)
        {
            Sprites.Clear();

            if (framesPerColumn == null || framesPerColumn.Length != columns)
                throw new ArgumentException("framesPerColumn 长度应等于 columns");

            for (int c = 0; c < columns; c++)
            {
                int frames = Math.Max(1, framesPerColumn[c]);

                int colX = start.X + c * (columnWidth + colSpacing.X);
                int colY = start.Y;

                // 每列的第一帧矩形
                var first = new Rectangle(colX, colY, columnWidth, frameHeight);
                // 纵向逐帧
                var step = new Point(0, frameHeight + colSpacing.Y);

                Sprites.Add(new SpriteDef(first, step, frames));
            }

            return this;
        }

        /// <summary>
        /// 手动加入一个不规则的精灵（第一帧矩形 + 帧数 + 帧步进）
        /// </summary>
        public SpriteSheet AddSprite(Rectangle firstFrame, Point frameStep, int frameCount)
        {
            Sprites.Add(new SpriteDef(firstFrame, frameStep, frameCount));
            return this;
        }

        /// <summary>
        /// 一次性加入多条不规则精灵
        /// </summary>
        public SpriteSheet AddSprites(IEnumerable<SpriteDef> defs)
        {
            Sprites.AddRange(defs);
            return this;
        }

        public Rectangle GetFrameRect(int spriteIndex, int frameIndex)
        {
            if (spriteIndex < 0 || spriteIndex >= Sprites.Count)
                return new Rectangle(0, 0, 1, 1);
            return Sprites[spriteIndex].GetFrameRect(frameIndex);
        }

        /// <summary>
        /// 直接绘制（可选）。你也可以自己拿 GetFrameRect 去调用 Main.EntitySpriteDraw。
        /// </summary>
        public void Draw(int spriteIndex, int frameIndex, Vector2 worldCenter, Color color, float rotation, float scale, SpriteEffects fx = SpriteEffects.None, float layerDepth = 0f)
        {
            var rect = GetFrameRect(spriteIndex, frameIndex);
            var origin = new Vector2(rect.Width / 2f, rect.Height / 2f);

            Main.EntitySpriteDraw(Texture,
                position: worldCenter - Main.screenPosition,
                sourceRectangle: rect,
                color: color,
                rotation: rotation,
                origin: origin,
                scale: scale,
                effects: fx);
        }
    }
}
