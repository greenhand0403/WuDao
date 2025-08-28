// Content/Tiles/WishingWellTile.cs
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace WuDao.Content.Tiles
{
    public class WishingWellTile : ModTile
    {
        // 如需直接引用原版贴图，可取消注释（不同版本路径可能不同，若编译报错，请改用自带贴图）：
        // public override string Texture => $"Terraria/Images/Tiles_219"; // Extractinator
        // 明确告知：没有智能交互（不占右键）
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            // 关键：不要抢右键/智能交互，否则会影响右键分堆
            TileID.Sets.DisableSmartInteract[Type] = true;
            TileID.Sets.HasOutlines[Type] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.addTile(Type);
            // ✅ 动画帧高度（3行 * 18px）
            AnimationFrameHeight = 54;
            // 交给引擎处理掉落，避免多掉
            RegisterItemDrop(ModContent.ItemType<Items.WishingWellItem>());

            AddMapEntry(new Color(100, 149, 237));
        }
        // ✅ 每块独立决定使用哪一帧
        public override void AnimateIndividualTile(int type, int i, int j, ref int frameX, ref int frameY)
        {
            // 定位这口井的 TE（左上角坐标）
            Tile t = Framing.GetTileSafely(i, j);
            // 每格 18px，3×3 物件，所以对 3 取模
            int left = i - (t.TileFrameX / 18) % 3;
            int top = j - (t.TileFrameY / 18) % 3;
            var pos = new Point16(left, top);

            if (TileEntity.ByPosition.TryGetValue(pos, out var te) && te is WishingWellTE well)
            {
                if (well.IsReady)
                {
                    // ✅ 可用：跟随原版提炼机的动画帧
                    // Main.tileFrame[TileID.Extractinator] 是“第几帧”
                    int vanillaFrame = Main.tileFrame[TileID.Extractinator];
                    frameY = vanillaFrame * AnimationFrameHeight;
                }
                else
                {
                    // ❄ 冷却中：静止在第 0 帧
                    frameY = 0;
                }
            }
            else
            {
                // 没有 TE（极少数异常）：保持静止
                frameY = 0;
            }
        }
        
        public override void PlaceInWorld(int i, int j, Item item)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            // 以左上为 TE 坐标
            int left = i - 1;
            int top = j - 2;
            ModContent.GetInstance<WishingWellTE>().Place(left, top);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            int left = i - (frameX / 18) % 3;
            int top = j - (frameY / 18) % 3;
            var pos = new Point16(left, top);
            ModContent.GetInstance<WishingWellTE>().Kill(left, top);
        }
    }
}
