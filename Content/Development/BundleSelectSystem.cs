// ======================== BundleSelectSystem.cs ========================
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace WuDao.Content.Development
{
    public class BundleSelectSystem : ModSystem
    {
        private static UserInterface _ui;
        private static BundleSelectUI _state;

        private static bool _visible;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                _ui = new UserInterface();
                _state = new BundleSelectUI();
            }
        }

        public override void Unload()
        {
            _ui = null;
            _state = null;
        }

        /// <summary>显示 UI，并注册点击后的回调。</summary>
        public static void Show(Action<BundleCategory> onSelect)
        {
            if (Main.dedServ || _ui == null || _state == null) return;

            _state.RemoveAllChildren();
            _state = new BundleSelectUI { OnSelect = onSelect };
            _state.Activate();

            _ui?.SetState(_state);
            _visible = true;
        }

        public static void Hide()
        {
            if (Main.dedServ || _ui == null) return;
            _ui?.SetState(null);
            _visible = false;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (_visible)
                _ui?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
        {
            if (!_visible || _ui == null) return;

            int idx = layers.FindIndex(l => l.Name.Equals("Vanilla: Inventory"));
            if (idx != -1)
            {
                layers.Insert(idx + 1, new LegacyGameInterfaceLayer(
                    "WuDao: BundleSelectUI",
                    delegate
                    {
                        _ui.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
