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

            // —— 尺寸（随 UI 缩放）——
            float scale = Main.inventoryScale;
            int slotW = (int)(TextureAssets.InventoryBack.Value.Width * scale);
            int slotH = (int)(TextureAssets.InventoryBack.Value.Height * scale);

            // —— Vanilla 背包的“底边 y”——
            int inventoryBottom = Main.instance.invBottom;

            int leftPadding = (int)(16 * scale); // 适配不同缩放

            // —— 目标位置：贴在“底行最左格”的正下方（略留间距）——
            int margin = (int)(8 * scale);
            int x = leftPadding + slotW / 2;//适当调整位置
            int y = inventoryBottom + margin;

            // —— 背板与标题（可选）——
            Rectangle slotRect = new Rectangle(x, y, slotW, slotH);
            // Rectangle panelRect = new Rectangle(x - 6, y - 6, slotW + 12, slotH + 12);
            // DrawRect(sb, panelRect, new Color(20, 20, 20, 160));
            // Utils.DrawBorderString(sb, "绝学栏", new Vector2(x - 4, y + 28), Color.Goldenrod);

            // —— 命中测试，阻断背包交互 —— 
            // if (slotRect.Contains(Main.mouseX, Main.mouseY))
            //     player.mouseInterface = true;

            // —— 绘制物品（ItemSlot.Draw 会使用 Main.inventoryScale）——
            ItemSlot.Draw(sb, ref qi.JuexueSlot, ItemSlot.Context.InventoryItem, new Vector2(x, y));

            if (slotRect.Contains(Main.mouseX, Main.mouseY))
            {
                var mouse = Main.mouseItem;
                bool mouseIsJuexue = !mouse.IsAir && mouse.ModItem is JuexueItem;

                // —— 左键交换：你的原逻辑不变 ——
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (mouse.IsAir)
                    {
                        Main.mouseItem = qi.JuexueSlot.Clone();
                        qi.JuexueSlot.TurnToAir();
                    }
                    else if (mouseIsJuexue)
                    {
                        Utils.Swap(ref Main.mouseItem, ref qi.JuexueSlot);
                    }
                    else
                    {
                        Main.NewText("该槽位只能放入“绝学”类物品。", Color.OrangeRed);
                    }
                    Main.mouseLeftRelease = false;
                }
                // —— 右键：从绝学栏卸下到背包空格 ——
                // 放在左键交换处理之后
                if (Main.mouseRight && Main.mouseRightRelease)
                {
                    if (!qi.JuexueSlot.IsAir)
                    {
                        // 寻找主背包空格（0..49：含热键栏与主包，不含钱币/弹药栏）
                        int emptyIndex = -1;
                        for (int i = 0; i <= 49; i++)
                        {
                            if (player.inventory[i].IsAir) { emptyIndex = i; break; }
                        }

                        if (emptyIndex != -1)
                        {
                            // 放入空格，并清空绝学槽
                            player.inventory[emptyIndex] = qi.JuexueSlot.Clone();
                            qi.JuexueSlot.TurnToAir();

                            // 可选：音效反馈
                            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Grab, player.Center);
                        }
                        else
                        {
                            Main.NewText("背包已满，无法卸下绝学。", Microsoft.Xna.Framework.Color.OrangeRed);
                        }
                    }

                    // 阻断右键连击与本帧交互冒泡
                    Main.mouseRightRelease = false;
                }
                // 默认显示
                Main.HoverItem = new Item();
                Main.hoverItemName = "放入一件绝学以激活其效果";
                // 这句话会在拿着绝学物品，光标移到绝学槽时显示
                Main.instance.MouseText("将绝学放置到绝学槽中");

                if (!qi.JuexueSlot.IsAir)
                {
                    Main.HoverItem = qi.JuexueSlot.Clone();
                    Main.hoverItemName = "可以装备绝学";
                }

                // 阻断其它 UI 交互
                player.mouseInterface = true;
            }

            // —— 冗余防御 —— 
            // if (!qi.JuexueSlot.IsAir && qi.JuexueSlot.ModItem is not JuexueItem)
            // {
            //     if (Main.mouseItem.IsAir) Main.mouseItem = qi.JuexueSlot.Clone();
            //     else player.QuickSpawnItem(player.GetSource_Misc("QiSlotReject"), qi.JuexueSlot.Clone());
            //     qi.JuexueSlot.TurnToAir();
            //     Main.NewText("该槽位只能放入“绝学”类物品。", Color.OrangeRed);
            // }
        }

        private static void DrawRect(SpriteBatch sb, Rectangle r, Color c)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            sb.Draw(tex, r, c);
        }
    }
}
