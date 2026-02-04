using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using WuDao.Content.Projectiles.Throwing.Base; // 引入我们做的基础射弹类

namespace WuDao.Content.Projectiles.Throwing
{
    // 黄金飞镖
    public class GoldenShurikenProjectile : BaseShurikenProjectile
    {
        // 使用物品贴图（原文件就是这么做的）
        public override string Texture => "WuDao/Content/Items/Weapons/Throwing/GoldenShuriken";

        // ===== 基类配置项 =====
        protected override int TrailLength => 5;            // 拖尾长度
        protected override int TrailMode => 0;              // 线性拖尾
        protected override int ExtraUpdates => 1;           // 提升平滑度
        protected override int TimeLeft => 300;             // 生命周期
        protected override int InitialAlpha => 255;         // 初始透明
        protected override int AlphaFadePerTick => 0;       // 不用基类淡入，原版依赖AIType的alpha衰减

        protected override int HitDustType => DustID.GoldFlame;
        protected override int HitDustCount => 10;
        protected override SoundStyle? HitSound => SoundID.Dig;

        protected override int TileDustType => DustID.GoldFlame;
        protected override int TileDustCount => 10;
        protected override SoundStyle? TileHitSound => SoundID.Dig;

        protected override int DebuffTypeOnHit => BuffID.Midas;
        protected override int DebuffTimeOnHit => 120;      // 2秒 Midas
        protected override int DebuffChanceDenom => 3;      // 1/3 概率

        protected override bool DrawMainSpriteInPreDraw => true;

        // 如果想额外做什么（这里用Dust模拟你原来的CreateGoldenDusts），可以在命中扩展里加
        protected override void OnHitNPCExt(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 10; i++)
            {
                CreateGoldenDusts(Projectile.position, Projectile.velocity);
            }
        }

        protected override bool OnTileCollideExt(Vector2 oldVelocity)
        {
            for (int i = 0; i < 10; i++)
            {
                CreateGoldenDusts(Projectile.position, Projectile.velocity);
            }
            return true; // 按原逻辑，碰到方块销毁
        }

        private void CreateGoldenDusts(Vector2 pos, Vector2 vel)
        {
            Dust.NewDustDirect(pos, Projectile.width, Projectile.height,
                DustID.GoldFlame,
                vel.X * 0.1f,
                vel.Y * 0.1f,
                100, default, 0.85f);
        }
    }
}
