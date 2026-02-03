using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace WuDao.Content.Development
{
    // 开局礼包选择UI菜单
    public class BundleSelectUI : UIState
    {
        public Action<BundleCategory> OnSelect;

        private UIPanel _panel;
        private UIList _list;
        private UIScrollbar _scrollbar;
        private UIText _title;

        // ===== 样式 / 布局常量 =====
        private const float PanelMinW = 320f;
        private const float PanelMaxW = 640f;
        private const float PanelPad = 12f;   // 面板内边距
        private const float TitleTop = 10f;   // 标题距顶部
        private const float BtnHeight = 40f;   // 按钮高度
        private const float ListPadding = 10f; // 按钮之间的间距
        private const float ScrollbarW = 20f;

        // 我们自己记录按钮数量，避免去索引 UIList
        private int _itemCount = 0;
        private bool _layoutDirty;

        public override void OnInitialize()
        {
            _panel = new UIPanel
            {
                HAlign = 0.5f,
                VAlign = 0.35f
            };
            Append(_panel);

            _title = new UIText("选择要领取的类别", 0.9f) { HAlign = 0.5f };
            _title.Top.Set(TitleTop, 0f);
            _panel.Append(_title);

            _list = new UIList
            {
                ListPadding = ListPadding
            };
            // 宽度：填满面板（左右留出 PanelPad），还要预留滚动条宽度
            _list.Left.Set(PanelPad, 0f);
            _list.Top.Set(TitleTop + 28f, 0f);
            _list.Width.Set(-PanelPad * 2f - ScrollbarW, 1f);
            _panel.Append(_list);

            _scrollbar = new UIScrollbar();
            _scrollbar.Width.Set(ScrollbarW, 0f);
            _scrollbar.HAlign = 1f;
            _panel.Append(_scrollbar);
            _list.SetScrollbar(_scrollbar);

            // ===== 你的三个选项 =====
            AddButton("武器（含弹药）", () => Select(BundleCategory.Weapons));
            AddButton("饰品", () => Select(BundleCategory.Accessories));
            AddButton("其他", () => Select(BundleCategory.Others));

            _layoutDirty = true;
        }

        private void AddButton(string text, Action onClick)
        {
            var btn = new UITextPanel<string>(text, 0.9f, true)
            {
                HAlign = 0.5f
            };
            // 宽度跟随 _list（这里不用再减滚动条，因为 _list 已经预留了）
            btn.Width.Set(0f, 1f);
            btn.Height.Set(BtnHeight, 0f);
            btn.OnLeftClick += (_, __) =>
            {
                SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
                onClick?.Invoke();
            };

            _list.Add(btn);
            _itemCount++;
            _layoutDirty = true;
        }

        private void Select(BundleCategory cat)
        {
            OnSelect?.Invoke(cat);
        }

        public override void OnActivate()
        {
            base.OnActivate();
            _layoutDirty = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Esc 关闭
            if (Main.keyState.IsKeyDown(Keys.Escape) && Main.oldKeyState.IsKeyUp(Keys.Escape))
            {
                BundleSelectSystem.Hide();
                return;
            }

            // ===== UIScale 感知：UI 坐标应使用“屏幕尺寸 / UIScale” =====
            float uiWidth = Main.screenWidth / Main.UIScale;
            float uiHeight = Main.screenHeight / Main.UIScale;

            // 自适应面板宽度：屏宽的 35%，并夹在最小/最大范围内
            float desiredW = MathHelper.Clamp(uiWidth * 0.35f, PanelMinW, PanelMaxW);
            if (Math.Abs(_panel.Width.Pixels - desiredW) > 0.5f)
            {
                _panel.Width.Set(desiredW, 0f);
                _layoutDirty = true;
            }

            if (_layoutDirty)
            {
                // 估算内容高度 = 标题 +（按钮总高 + 间距）+ 底部 padding
                float contentListH = _itemCount > 0
                    ? _itemCount * BtnHeight + (_itemCount - 1) * ListPadding
                    : 0f;

                float contentH = TitleTop + 28f + contentListH + PanelPad;

                // 面板高度 = min(内容高度, 屏幕 60% 高)，并给一个下限避免太小
                float maxH = uiHeight * 0.60f;
                float desiredH = MathHelper.Clamp(contentH, 160f, maxH);

                _panel.Height.Set(desiredH, 0f);

                // 列表高度：去掉标题占用与底部 padding
                float listH = desiredH - (TitleTop + 28f) - PanelPad;
                if (listH < 0f) listH = 0f;

                _list.Height.Set(listH, 0f);

                // 滚动条位置/高度与列表一致
                _scrollbar.Top.Set(_list.Top.Pixels, 0f);
                _scrollbar.Height.Set(listH, 0f);

                Recalculate();
                _layoutDirty = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            // 吸附鼠标，防止穿透点击其他 UI
            if (ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }
    }
}
