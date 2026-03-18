using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Systems
{
    public class InvisibleSwordQiSystem : ModSystem
    {
        private static RenderTarget2D _maskTarget;
        private static Texture2D _maskTexture;
        private static bool _hooksLoaded;
        public override void Load()
        {
            if (Main.dedServ)
                return;

            _maskTexture = ModContent.Request<Texture2D>(
                "WuDao/Assets/InvisibleSwordQiMask",
                AssetRequestMode.ImmediateLoad
            ).Value;

            On_FilterManager.EndCapture += OnEndCapture;
            On_Main.InitTargets_int_int += OnInitTargets;
            On_Main.EnsureRenderTargetContent += OnEnsureRenderTargetContent;
            _hooksLoaded = true;
        }

        public override void Unload()
        {
            if (_hooksLoaded)
            {
                On_FilterManager.EndCapture -= OnEndCapture;
                On_Main.InitTargets_int_int -= OnInitTargets;
                On_Main.EnsureRenderTargetContent -= OnEnsureRenderTargetContent;
                _hooksLoaded = false;
            }

            // 不要在这里 Dispose 图形资源
            _maskTexture = null;
            _maskTarget = null;
        }

        private void OnInitTargets(On_Main.orig_InitTargets_int_int orig, Main self, int width, int height)
        {
            orig(self, width, height);
            RecreateTargetIfNeeded();
        }

        private void OnEnsureRenderTargetContent(On_Main.orig_EnsureRenderTargetContent orig, Main self)
        {
            orig(self);
            RecreateTargetIfNeeded();
        }

        private static void RecreateTargetIfNeeded()
        {
            if (Main.dedServ)
                return;

            GraphicsDevice gd = Main.instance.GraphicsDevice;

            bool needRecreate =
                _maskTarget == null ||
                _maskTarget.IsDisposed ||
                _maskTarget.Width != Main.screenWidth ||
                _maskTarget.Height != Main.screenHeight;

            if (needRecreate)
            {
                if (_maskTarget != null && !_maskTarget.IsDisposed)
                    _maskTarget.Dispose();

                _maskTarget = new RenderTarget2D(
                    gd,
                    Main.screenWidth,
                    Main.screenHeight,
                    false,
                    gd.PresentationParameters.BackBufferFormat,
                    DepthFormat.None
                );
            }
        }

        private void OnEndCapture(
            On_FilterManager.orig_EndCapture orig,
            FilterManager self,
            RenderTarget2D finalTexture,
            RenderTarget2D screenTarget1,
            RenderTarget2D screenTarget2,
            Color clearColor)
        {
            if (Main.dedServ || _maskTarget == null || _maskTexture == null || WuDao.InvisibleSwordQiEffect == null)
            {
                orig(self, finalTexture, screenTarget1, screenTarget2, clearColor);
                return;
            }

            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            SpriteBatch sb = Main.spriteBatch;

            int swordQiType = ModContent.ProjectileType<InvisibleSwordQiProj>();
            bool hasSwordQi = false;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == swordQiType)
                {
                    hasSwordQi = true;
                    break;
                }
            }

            if (!hasSwordQi)
            {
                orig(self, finalTexture, screenTarget1, screenTarget2, clearColor);
                return;
            }

            // -------------------------------------------------
            // 1. 先把当前已经渲染好的场景保存到 Main.screenTargetSwap
            // 这一步是黑洞案例最关键的地方
            // -------------------------------------------------
            gd.SetRenderTarget(Main.screenTargetSwap);
            gd.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer);
            sb.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            sb.End();

            // -------------------------------------------------
            // 2. 把所有剑气绘制到遮罩 RT
            // 这里只画遮罩，不画到主屏幕
            // -------------------------------------------------
            gd.SetRenderTarget(_maskTarget);
            gd.Clear(Color.Transparent);

            sb.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            Vector2 origin = _maskTexture.Size() * 0.5f;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.type != swordQiType)
                    continue;

                float lifeFade = 1f;
                if (proj.timeLeft < 10)
                    lifeFade = proj.timeLeft / 10f;

                // 这里只让 alpha 进 mask，不要画到主屏幕
                float alpha = 0.75f * lifeFade;

                // 主体
                sb.Draw(
                    _maskTexture,
                    proj.Center - Main.screenPosition,
                    null,
                    Color.White * alpha,
                    proj.rotation,
                    origin,
                    proj.scale,
                    SpriteEffects.None,
                    0f
                );

                // 轻微拖尾，增强局部范围
                // for (int k = 0; k < 4; k++)
                // {
                //     Vector2 trailPos = proj.Center - proj.velocity * (k + 1) * 0.35f;
                //     float trailAlpha = alpha * (0.35f - k * 0.07f);
                //     if (trailAlpha <= 0f)
                //         continue;

                //     sb.Draw(
                //         _maskTexture,
                //         trailPos - Main.screenPosition,
                //         null,
                //         Color.White * trailAlpha,
                //         proj.rotation,
                //         origin,
                //         proj.scale * (1f - k * 0.04f),
                //         SpriteEffects.None,
                //         0f
                //     );
                // }
            }

            sb.End();

            // -------------------------------------------------
            // 3. 回到主屏幕 RT，先把原场景画回去
            // -------------------------------------------------
            gd.SetRenderTarget(Main.screenTarget);
            gd.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer);
            sb.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            sb.End();

            // -------------------------------------------------
            // 4. 全屏 shader：s0=原场景, s1=剑气遮罩
            // 只在 mask 区域做局部扭曲
            // -------------------------------------------------
            sb.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                Main.Rasterizer
            );

            gd.Textures[0] = Main.screenTargetSwap;
            gd.Textures[1] = _maskTarget;

            Effect effect = WuDao.InvisibleSwordQiEffect;
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["uDistortStrength"]?.SetValue(0.02f);
            effect.Parameters["uScreenSize"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));

            effect.CurrentTechnique.Passes[0].Apply();

            sb.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            sb.End();

            orig(self, finalTexture, screenTarget1, screenTarget2, clearColor);
        }
    }
}