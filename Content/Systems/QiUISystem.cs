using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent;
using Terraria.UI.Chat;
using WuDao.Content.Players;
using WuDao.Content.UI;
using WuDao.Content.Juexue.Base;

namespace WuDao.Content.Systems
{
    // 负责：顶部中间的气力条；背包打开时在物品栏下方显示“绝学槽”
    public class QiUISystem : ModSystem
    {
        public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
        {
            int idx = layers.FindIndex(l => l.Name.Equals("Vanilla: Interface Logic 1"));
            if (idx != -1)
            {
                layers.Insert(idx + 1, new LegacyGameInterfaceLayer(
                    "QiJuexue: QiBar",
                    delegate
                    {
                        DrawQiBar();
                        return true;
                    }, InterfaceScaleType.UI)
                );
            }

            // 背包中的绝学槽：放在背包层之后
            int invIdx = layers.FindIndex(l => l.Name.Equals("Vanilla: Inventory"));
            if (invIdx != -1)
            {
                layers.Insert(invIdx + 1, new LegacyGameInterfaceLayer(
                    "QiJuexue: JuexueSlot",
                    delegate
                    {
                        DrawJuexueSlot();
                        return true;
                    }, InterfaceScaleType.UI)
                );
            }
        }

        private void DrawQiBar()
        {
            var player = Main.LocalPlayer;
            var qi = player.GetModPlayer<QiPlayer>();
            if (!qi.ShouldShowQiBar() || qi.QiMax <= 0) return;

            // 顶部中间
            int barWidth = 260;
            int barHeight = 18;
            int x = Main.screenWidth / 2 - barWidth / 2;
            int y = 16;

            QiBarDrawer.DrawSimpleBar(new Rectangle(x, y, barWidth, barHeight), qi.QiCurrent, qi.QiMax, "气力");
        }

        private void DrawJuexueSlot()
        {
            if (!Main.playerInventory) return;

            var player = Main.LocalPlayer;
            var qi = player.GetModPlayer<QiPlayer>();
            var sb = Main.spriteBatch;

            // === 按“第6行第1列”定位 ===
            float scale = Main.inventoryScale; // 通常 0.85
            int slotW = (int)(TextureAssets.InventoryBack.Value.Width * scale);
            int slotH = (int)(TextureAssets.InventoryBack.Value.Height * scale);

            int columns = 29;
            int gridWidth = columns * slotW;
            int gridLeft = Main.screenWidth / 2 - gridWidth;

            // 重点：invBottom 是第4行的“底边”，先反推首行，再取第6行
            int baseTop = Main.instance.invBottom - 4 * slotH; // 背包首行(第1行)顶边
            int rowIndex = 5;   // 第6行（从0开始）
            int colIndex = 0;   // 第1列（从0开始）
            int x = gridLeft + colIndex * slotW;
            int y = baseTop + rowIndex * slotH - 2;

            // 背板 & 标题
            Rectangle slotRect = new Rectangle(x, y, slotW, slotH);
            Rectangle panelRect = new Rectangle(x - 6, y - 6, slotW + 12, slotH + 12);

            DrawRect(sb, panelRect, new Color(20, 20, 20, 160));
            // 稍微往左上移动一点点
            Utils.DrawBorderString(sb, "绝学栏", new Vector2(x - 4, y - 28), Color.Goldenrod);

            // 在自定义槽上悬浮：阻断背包默认交互
            if (slotRect.Contains(Main.mouseX, Main.mouseY))
            {
                player.mouseInterface = true;
            }

            // 绘制物品（仅负责显示，不用 Handle）
            ItemSlot.Draw(sb, ref qi.JuexueSlot, ItemSlot.Context.InventoryItem, new Vector2(x, y));

            // —— 手动处理鼠标交互（左键交换/放入/取出）——
            if (slotRect.Contains(Main.mouseX, Main.mouseY) && Main.mouseLeft && Main.mouseLeftRelease)
            {
                // 只允许放“绝学”或拿起；如果鼠标为空则拿起，如果鼠标有物品则尝试放入
                var mouse = Main.mouseItem;

                bool mouseIsJuexue = !mouse.IsAir && mouse.ModItem is JuexueItem;
                bool slotIsJuexue = !qi.JuexueSlot.IsAir && qi.JuexueSlot.ModItem is JuexueItem;

                if (mouse.IsAir)
                {
                    // 鼠标空：从槽位拿起（不限制类型，因为槽里只可能有绝学或空）
                    Main.mouseItem = qi.JuexueSlot.Clone();
                    qi.JuexueSlot.TurnToAir();
                }
                else if (mouseIsJuexue)
                {
                    // 鼠标拿着绝学：与槽交换
                    Utils.Swap(ref Main.mouseItem, ref qi.JuexueSlot);
                }
                else
                {
                    // 鼠标不是绝学，拒绝并提示
                    Main.NewText("该槽位只能放入“绝学”类物品。", Color.OrangeRed);
                }

                // 吞掉这次点击，避免传导到背包空格导致“又放回去”
                Main.mouseLeftRelease = false;
                player.mouseInterface = true;
            }

            // —— 冗余防御：如果槽里被塞了非绝学（理论上不会出现），弹回鼠标 —— 
            if (!qi.JuexueSlot.IsAir && qi.JuexueSlot.ModItem is not JuexueItem)
            {
                if (Main.mouseItem.IsAir) Main.mouseItem = qi.JuexueSlot.Clone();
                else player.QuickSpawnItem(player.GetSource_Misc("QiSlotReject"), qi.JuexueSlot.Clone());
                qi.JuexueSlot.TurnToAir();
                Main.NewText("该槽位只能放入“绝学”类物品。", Color.OrangeRed);
            }
        }

        private static void DrawRect(SpriteBatch sb, Rectangle r, Color c)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            sb.Draw(tex, r, c);
        }
    }
}
