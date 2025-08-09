using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Throwing.Base
{
    /// <summary>
    /// 通用飞镖射弹基类（1.4）
    /// - 复用原版镖的 AI（CloneDefaults + AIType）
    /// - 可配置：拖尾、extraUpdates、timeLeft、alpha 淡入
    /// - 可配置：命中/碰墙的 Dust 与音效、几率 Debuff
    /// - 提供 PreDraw 拖尾渲染与颜色控制
    /// - 提供 ExtraAI/OnHitNPCExt 等扩展点
    /// </summary>
    public abstract class BaseShurikenProjectile : ModProjectile
    {
        // ====== 可在子类覆写的“基础属性” ======
        protected virtual int CloneFromProjectileID => ProjectileID.Shuriken;
        protected virtual int AICopyFromProjectileID => ProjectileID.Shuriken;

        protected virtual int TrailLength => 0;            // 0 表示不开启拖尾
        protected virtual int TrailMode => 0;              // 0 线性；参考原版枚举
        protected virtual int ExtraUpdates => 0;           // >0 会更丝滑
        protected virtual int TimeLeft => 300;             // 生命周期
        protected virtual int InitialAlpha => 0;           // 255 可实现淡入
        protected virtual int AlphaFadePerTick => 0;       // >0 表示每 tick 递减 alpha

        // 命中/碰撞视觉 & 音效
        protected virtual int HitDustType => -1;           // -1 不产尘
        protected virtual int HitDustCount => 0;
        protected virtual SoundStyle? HitSound => null;

        protected virtual int TileDustType => -1;
        protected virtual int TileDustCount => 0;
        protected virtual SoundStyle? TileHitSound => null;

        // 命中附加 Debuff（几率与时间）
        protected virtual int DebuffTypeOnHit => 0;        // 0 不上 Debuff
        protected virtual int DebuffTimeOnHit => 0;        // 以 tick 计
        protected virtual int DebuffChanceDenom => 1;      // 1 = 100% 触发；3 = 1/3 概率

        // 轻微旋转 & 阻力（在原版 AI 基础上加味）
        protected virtual float RotationSpeed => 0f;
        protected virtual float ExtraDrag => 0f;

        // 是否由基类在 PreDraw 里帮你画本体贴图（true 使用默认 Draw，false 只画拖尾、子类自行绘制本体）
        protected virtual bool DrawMainSpriteInPreDraw => true;

        // 纹理默认用射弹贴图；如果你用“物品贴图当射弹”，可在子类里 override 为物品路径
        public override string Texture => base.Texture;

        public override void SetStaticDefaults()
        {
            if (TrailLength > 0)
            {
                ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
                ProjectileID.Sets.TrailingMode[Projectile.type] = TrailMode;
            }
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(CloneFromProjectileID);
            AIType = AICopyFromProjectileID;

            Projectile.extraUpdates = ExtraUpdates;
            Projectile.timeLeft = TimeLeft;
            Projectile.alpha = InitialAlpha;

            SetExtraDefaults();
        }

        /// <summary>子类需要改穿透/尺寸等时覆写。</summary>
        protected virtual void SetExtraDefaults() { }

        public override void AI()
        {
            // 自旋（不影响原版的 AI 行为）
            if (RotationSpeed != 0f)
                Projectile.rotation += RotationSpeed * Projectile.direction;

            // 额外阻力
            if (ExtraDrag > 0f)
                Projectile.velocity *= (1f - ExtraDrag);

            // alpha 淡入
            if (AlphaFadePerTick > 0 && Projectile.alpha > 0)
                Projectile.alpha = Utils.Clamp(Projectile.alpha - AlphaFadePerTick, 0, 255);

            ExtraAI();
        }

        /// <summary>写自定义轨迹/粒子/光照等。</summary>
        protected virtual void ExtraAI() { }

        // 命中 NPC：加 Debuff + 自定义扩展
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (DebuffTypeOnHit > 0 && DebuffTimeOnHit > 0)
            {
                bool apply = DebuffChanceDenom <= 1 || Main.rand.NextBool(DebuffChanceDenom);
                if (apply)
                    target.AddBuff(DebuffTypeOnHit, DebuffTimeOnHit);
            }

            if (HitSound.HasValue)
                SoundEngine.PlaySound(HitSound.Value, Projectile.position);

            SpawnDust(HitDustType, HitDustCount);

            OnHitNPCExt(target, hit, damageDone);
        }

        /// <summary>命中扩展（分裂/溅射等）。</summary>
        protected virtual void OnHitNPCExt(NPC target, NPC.HitInfo hit, int damageDone) { }

        // 碰到地形：播粒子和音效；返回 true = 按默认处理（一般为销毁）
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (TileHitSound.HasValue)
                SoundEngine.PlaySound(TileHitSound.Value, Projectile.position);

            SpawnDust(TileDustType, TileDustCount);

            return OnTileCollideExt(oldVelocity);
        }

        /// <summary>需要反弹而非销毁时可覆写返回 false 并手动处理速度。</summary>
        protected virtual bool OnTileCollideExt(Vector2 oldVelocity) => true;

        // ====== 拖尾 & 绘制 ======
        public override bool PreDraw(ref Color lightColor)
        {
            // 画拖尾
            if (TrailLength > 0 && Projectile.oldPos != null)
            {
                Texture2D tex = TextureAssets.Projectile[Type].Value;
                Vector2 origin = tex.Size() * 0.5f;

                for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;

                    float t = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length; // 0..1
                    Color c = GetTrailColor(lightColor, t);

                    Vector2 drawPos = Projectile.oldPos[i] + origin - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                    Main.EntitySpriteDraw(tex, drawPos, null, c, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
                }
            }

            if (!DrawMainSpriteInPreDraw)
                return false; // 子类自己画主体

            return true; // 交回默认流水线画主体
        }

        /// <summary>根据插值 t(0..1) 返回拖尾颜色，子类可覆写做渐变/脉冲。</summary>
        protected virtual Color GetTrailColor(Color lightColor, float t)
        {
            // 默认白色渐隐
            return new Color(255, 255, 255, 255) * t;
        }

        // ====== 小工具 ======
        protected void SpawnDust(int dustType, int count)
        {
            if (dustType < 0 || count <= 0) return;
            for (int i = 0; i < count; i++)
            {
                Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    dustType,
                    Projectile.velocity.X * 0.1f,
                    Projectile.velocity.Y * 0.1f,
                    100, default, 0.85f
                );
            }
        }
    }
}
